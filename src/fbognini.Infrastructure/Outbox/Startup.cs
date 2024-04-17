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

        return services;
    }


    public static IServiceCollection AddMediatROutboxMessagesProcessor(this IServiceCollection services, IConfiguration configuration)
        => services.AddOutboxMessagesProcessor<MediatROutboxMessagesProcessor>(configuration);

    public static IServiceCollection AddOutboxMessagesProcessor<TOutboxProcessor>(this IServiceCollection services, IConfiguration configuration)
        where TOutboxProcessor : class, IOutboxMessageProcessor
    {
        var outboxSection = configuration.GetSection("Outbox");
        services.Configure<OutboxSettings>(outboxSection);

        services.AddScoped<IOutboxMessageProcessor, TOutboxProcessor>();

        return services;
    }
}
