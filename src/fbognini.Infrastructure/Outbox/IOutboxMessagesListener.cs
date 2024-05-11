using fbognini.Application.Multitenancy;
using fbognini.Core.Domain;
using fbognini.Infrastructure.Entities;
using fbognini.Infrastructure.Persistence;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Outbox;

public interface IOutboxMessagesListener
{
    Task PublishDomainMemoryEventAsync(IReadOnlyList<IDomainMemoryEvent> domainMemoryEvents, CancellationToken cancellationToken);

    void Notify();
}

internal class OutboxMessagesListenerService<TTenant> : BackgroundService, IOutboxMessagesListener
    where TTenant : Tenant, new()
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxMessagesListenerService<TTenant>> logger;

    private CancellationTokenSource _wakeupCancelationTokenSource = new();

    public OutboxMessagesListenerService(IServiceScopeFactory scopeFactory, ILogger<OutboxMessagesListenerService<TTenant>> logger)
    {
        _scopeFactory = scopeFactory;
        this.logger = logger;
    }

    public async Task PublishDomainMemoryEventAsync(IReadOnlyList<IDomainMemoryEvent> domainMemoryEvents, CancellationToken cancellationToken)
    {
        if (domainMemoryEvents is null || domainMemoryEvents.Count == 0)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var publisher = scope.ServiceProvider.GetService<IOutboxMessagePublisher>();
        if (publisher is null)
        {
            logger.LogWarning("No IOutboxMessagePublisher has been registered, memory events can't be processed");
            return;
        }

        foreach (var domainMemoryEvent in domainMemoryEvents)
        {
            await publisher.Publish(domainMemoryEvent, cancellationToken);
        }
    }

    public void Notify()
    {
        logger.LogDebug("Listener has been notified");

        _wakeupCancelationTokenSource.Cancel();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int interval = -1;

        using (var scope = _scopeFactory.CreateScope())
        {
            var applicationName = Assembly.GetEntryAssembly()!.GetName().Name!;

            var outboxSettings = scope.ServiceProvider.GetRequiredService<IOptions<OutboxSettings>>().Value;
            if (outboxSettings.ApplicationFilter == OutboxProcessorApplicationFilter.None)
            {
                logger.LogInformation("Outbox messages won't be processed by {ApplicationName}", applicationName);
                return;
            }

            var publisher = scope.ServiceProvider.GetService<IOutboxMessagePublisher>();
            if (publisher is null)
            {
                logger.LogWarning("No IOutboxMessagePublisher has been registered, outbox messages won't be processed by {ApplicationName}", applicationName);
                return;
            }

            if (outboxSettings.IntervalInSeconds > 0)
            {
                interval = outboxSettings.IntervalInSeconds * 1000;
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishDomainEvents(interval, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during domain events publishing");

                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task PublishDomainEvents(int interval, CancellationToken stoppingToken)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var baseDbContext = scope.ServiceProvider.GetRequiredService<IBaseDbContext>();
            var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();

            int noOfOutboxMessages;
            do
            {
                var dbContext = (DbContext)baseDbContext;

                using var transaction = dbContext.Database.BeginTransaction();

                noOfOutboxMessages = await processor.Process(stoppingToken);

                await transaction.CommitAsync(stoppingToken);
            }
            while (noOfOutboxMessages > 0);
        }

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_wakeupCancelationTokenSource.Token, stoppingToken);
        try
        {
            if (interval == -1)
            {
                logger.LogInformation("Listening for a new event");
            }
            else
            {
                logger.LogInformation("Listening for a new event of waiting for {Internval}ms", interval);
            }

            await Task.Delay(interval, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            if (_wakeupCancelationTokenSource.Token.IsCancellationRequested)
            {
                logger.LogInformation("Outbox listener is awake");

                var tmp = _wakeupCancelationTokenSource;
                _wakeupCancelationTokenSource = new CancellationTokenSource();
                tmp.Dispose();
            }
            else if (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Outbox listener is shutting down");
            }
        }
    }

}