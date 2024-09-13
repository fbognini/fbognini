using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Multitenancy;
using fbognini.Infrastructure.Persistence;
using SberemPay.Checkout.Services;
using WebApplication1.Infrastructure.Persistance;
using WebApplication1.Infrastructure.Repositorys;
using WebApplication1.Services;

namespace WebApplication1.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            services
                .AddRepositories()
                .AddPersistenceAndMultitenancy<WebApplication1DbContext>(configuration)
                .WithFakeMultitenancy();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped(typeof(IWebApplication1Repository), typeof(WebApplication1Repository));
        }
    }
}
