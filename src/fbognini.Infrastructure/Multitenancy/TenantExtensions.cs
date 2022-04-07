using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace fbognini.Infrastructure.Multitenancy;

public static class TenantExtensions
{
    public static IApplicationBuilder UseMiddlewareTenant(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantMiddleware>();
        return app;
    }

    public static IServiceCollection AddMiddlewareTenant(this IServiceCollection services)
    {
        services.AddScoped<TenantMiddleware>();
        return services;
    }
}