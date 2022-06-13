using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace fbognini.Infrastructure.Multitenancy
{

    public static class TenantExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<TenantMiddleware>();
            return app;
        }

        public static IServiceCollection AddTenantMiddleware(this IServiceCollection services)
        {
            services.AddScoped<TenantMiddleware>();
            return services;
        }
    }

}