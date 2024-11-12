using fbognini.Infrastructure.Entities;
using fbognini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Outbox;

public static class Startup
{
    public static IServiceCollection AddOutboxProcessing<TDbContext, TTenant>(this IServiceCollection services)
        where TDbContext : DbContext, IBaseDbContext
        where TTenant : Tenant, new()
    {
        return services
            .AddOutboxListener()
            .AddOutboxProcessor<TDbContext, TTenant>();
    }

    private static IServiceCollection AddOutboxListener(this IServiceCollection services)
    {
        services.AddSingleton<IOutboxMessagesListener, OutboxMessagesListenerService>();
        services.AddHostedService(sp => (OutboxMessagesListenerService)sp.GetRequiredService<IOutboxMessagesListener>());

        return services;
    }

    private static IServiceCollection AddOutboxProcessor<TDbContext, TTenant>(this IServiceCollection services)
        where TDbContext : DbContext, IBaseDbContext
        where TTenant : Tenant, new()
    {
        services.AddScoped<IOutboxMessageProcessor, OutboxMessageProcessor<TDbContext, TTenant>>();

        return services;
    }


    public static IServiceCollection AddMediatROutboxMessagesPublisher(this IServiceCollection services, IConfiguration configuration)
        => services.AddOutboxMessagesPublisher<MediatROutboxMessagesPublisher>(configuration);

    public static IServiceCollection AddOutboxMessagesPublisher<TOutboxPublisher>(this IServiceCollection services, IConfiguration configuration)
        where TOutboxPublisher : class, IOutboxMessagePublisher
    {
        var outboxSection = configuration.GetSection("Outbox");
        services.Configure<OutboxSettings>(outboxSection);

        services.AddScoped<IOutboxMessagePublisher, TOutboxPublisher>();

        return services;
    }
}
