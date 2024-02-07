using fbognini.Core.Domain;
using fbognini.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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

public class OutboxMessagesListenerService : BackgroundService, IOutboxMessagesListener
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxMessagesListenerService> logger;

    private CancellationTokenSource _wakeupCancelationTokenSource = new();
    private readonly string _applicationName = Assembly.GetEntryAssembly()!.GetName().Name!;

    public OutboxMessagesListenerService(IServiceScopeFactory scopeFactory, ILogger<OutboxMessagesListenerService> logger)
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
        var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();

        foreach (var domainMemoryEvent in domainMemoryEvents)
        {
            await processor.Process(domainMemoryEvent, cancellationToken);
        }
    }

    public void Notify()
    {
        _wakeupCancelationTokenSource.Cancel();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var outboxSettings = scope.ServiceProvider.GetRequiredService<IOptions<OutboxSettings>>().Value;
            if (outboxSettings.ApplicationFilter == OutboxProcessorApplicationFilter.None)
            {
                logger.LogInformation("Outbox messages won't be processed by {ApplicationName}", _applicationName);
                return;
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishDomainEvents(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during domain events publishing");

                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task PublishDomainEvents(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int interval = -1;

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IBaseDbContext>();
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();
                var outboxSettings = scope.ServiceProvider.GetRequiredService<IOptions<OutboxSettings>>().Value;
                if (outboxSettings.IntervalInSeconds > 0)
                {
                    interval = outboxSettings.IntervalInSeconds * 1000;
                }

                var outboxMessages = GetOutboxMessages(dbContext, outboxSettings);

                foreach (var outboxMessage in outboxMessages)
                {
                    try
                    {
                        await processor.Process(outboxMessage, stoppingToken);

                        outboxMessage.SetAsProcessed(DateTime.UtcNow);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(
                            exception,
                            "Exception while processing outbox message {MessageId}",
                            outboxMessage.Id);

                        outboxMessage.SetAsProcessedWithError(DateTime.UtcNow, exception.Message);
                    }

                    await dbContext.BaseSaveChangesAsync(stoppingToken);
                }
            }

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_wakeupCancelationTokenSource.Token, stoppingToken);
            try
            {
                await Task.Delay(interval, linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                if (_wakeupCancelationTokenSource.Token.IsCancellationRequested)
                {
                    logger.LogInformation("New events published");

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

    private List<OutboxMessage> GetOutboxMessages(IBaseDbContext dbContext, OutboxSettings outboxSettings)
    {
        var query = dbContext.OutboxMessages.Where(x => !x.ProcessedOnUtc.HasValue);

        if (outboxSettings.ApplicationFilter == OutboxProcessorApplicationFilter.Me)
        {
            query = query.Where(x => x.Application == _applicationName);
        }
        else if (outboxSettings.ApplicationFilter == OutboxProcessorApplicationFilter.Custom &&
            outboxSettings.Applications is not null)
        {
            var applications = outboxSettings.Applications.Split(new[] { ',', ';' }).ToList();
            query = query.Where(x => applications.Contains(x.Application));
        }

        var events = query
            .OrderBy(o => o.OccurredOnUtc)
            .Take(outboxSettings.BatchSize)
            .ToList();

        return events;
    }
}