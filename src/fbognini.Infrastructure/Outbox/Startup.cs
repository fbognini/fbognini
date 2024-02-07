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
    internal static IServiceCollection AddOutboxListener(this IServiceCollection services)
    {
        services.AddSingleton<IOutboxMessagesListener, OutboxMessagesListenerService>();

        return services;
    }


    public static IServiceCollection AddMediatROutboxMessagesProcessor(this IServiceCollection services, IConfiguration configuration) => services.AddOutboxMessagesProcessor<MediatROutboxMessagesProcessor>(configuration);

    public static IServiceCollection AddOutboxMessagesProcessor<TOutboxProcessor>(this IServiceCollection services, IConfiguration configuration)
        where TOutboxProcessor : class, IOutboxMessageProcessor
    {
        var outboxSection = configuration.GetSection("Outbox");
        services.Configure<OutboxSettings>(outboxSection);

        services.AddHostedService(sp => (OutboxMessagesListenerService)sp.GetRequiredService<IOutboxMessagesListener>());
        services.AddScoped<IOutboxMessageProcessor, TOutboxProcessor>();

        return services;
    }
}
