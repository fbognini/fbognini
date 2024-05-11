using fbognini.Infrastructure.Entities;
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
    internal static IServiceCollection AddOutboxListener<TTenant>(this IServiceCollection services)
        where TTenant : Tenant, new()
    {
        services.AddSingleton<IOutboxMessagesListener, OutboxMessagesListenerService<TTenant>>();
        services.AddHostedService(sp => (OutboxMessagesListenerService<TTenant>)sp.GetRequiredService<IOutboxMessagesListener>());

        services.AddScoped<IOutboxMessageProcessor, OutboxMessageProcessor<TTenant>>();

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
