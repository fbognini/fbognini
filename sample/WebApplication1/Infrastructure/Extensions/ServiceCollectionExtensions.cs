using fbognini.Application.Persistence;
using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Multitenancy;
using fbognini.Infrastructure.Persistence;
using fbognini.Infrastructure.Repositorys;
using SberemPay.Checkout.Services;
using WebApplication1.Application.Interfaces.Repositorys;
using WebApplication1.Infrastructure.Persistance;
using WebApplication1.Infrastructure.Repositorys;

namespace WebApplication1.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddPersistence(configuration)
                .AddMultitenancy(configuration).WithFakeStrategy();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped(typeof(IWebApplication1RepositoryAsync), typeof(WebApplication1RepositoryAsync))
                ;
        }

        private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddScoped<WebApplication1DbContext>()
                .AddPersistence<WebApplication1DbContext>(configuration)
                .AddRepositories()
                ;
        }
    }
}
