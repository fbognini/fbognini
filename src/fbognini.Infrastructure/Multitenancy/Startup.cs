using fbognini.Application.DependencyInjection;
using fbognini.Application.Entities;
using fbognini.Application.Multitenancy;
using fbognini.Infrastructure.Persistence;
using fbognini.Infrastructure.Persistence.Initialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Nager.PublicSuffix;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Multitenancy
{
    public static class Startup
    {
        private static FinbuckleMultiTenantBuilder<TTenant> AddBaseMultitenancy<TTenant>(this IServiceCollection services, IConfiguration configuration)
            where TTenant : Tenant, new()
        {
            // TODO: We should probably add specific dbprovider/connectionstring setting for the tenantDb with a fallback to the main databasesettings
            var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            string rootConnectionString = databaseSettings.ConnectionString;
            if (string.IsNullOrEmpty(rootConnectionString)) throw new InvalidOperationException("DB ConnectionString is not configured.");
            string dbProvider = databaseSettings.DBProvider;
            if (string.IsNullOrEmpty(dbProvider)) throw new InvalidOperationException("DB Provider is not configured.");

            services.AddApplication(Assembly.Load("fbognini.Application.Multitenancy"));

            return services
                .Configure<MultitenancySettings>(configuration.GetSection(nameof(MultitenancySettings)))
                .AddScoped<ITenantService, TenantService<TTenant>>()
                .AddTenantMiddleware()
                .AddMultiTenant<TTenant>();
        }

        public static FinbuckleMultiTenantBuilder<TTenant> AddMultitenancy<TTenantContext, TTenant>(this IServiceCollection services, IConfiguration configuration)
            where TTenantContext: TenantDbContext<TTenant>
            where TTenant : Tenant, new()

        {
            var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            if (databaseSettings.UseFakeMultitenancy)
            {
                throw new ArgumentException($"Cannot use {typeof(TTenantContext).Name} as TenantDbContext if UseFakeMultitenancy is enabled");
            }

            var builder = services
                    .AddBaseMultitenancy<TTenant>(configuration)
                    .WithEFCoreStore<TenantDbContext<TTenant>, TTenant>();

            services.AddDbContext<TTenantContext>(m => {
                m.UseSqlServer(databaseSettings.ConnectionString);
            });

            return builder;
        }

        public static FinbuckleMultiTenantBuilder<TTenant> AddMultitenancy<TTenant>(this IServiceCollection services, IConfiguration configuration)
            where TTenant : Tenant, new()
        {
            var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            if (databaseSettings.UseFakeMultitenancy)
            {
                return services
                    .AddBaseMultitenancy<TTenant>(configuration)
                    .WithInMemoryStore(options =>
                    {
                        options.IsCaseSensitive = true;
                        options.Tenants.Add(new TTenant
                        {
                            Identifier = MultitenancyConstants.Root.Key,
                            Name = MultitenancyConstants.Root.Name,
                            ConnectionString = databaseSettings.ConnectionString,
                            AdminEmail = MultitenancyConstants.Root.EmailAddress,
                            IsActive = true,
                            Issuer = null,
                            ValidUpto = DateTime.UtcNow.AddYears(1)
                        });
                    });
            }

            return services.AddMultitenancy<TenantDbContext<TTenant>, TTenant>(configuration);
        }

        public static FinbuckleMultiTenantBuilder<Tenant> AddMultitenancy(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddMultitenancy<Tenant>(configuration);
        }

        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();

            var initializers = serviceScope.ServiceProvider.GetServices<IMultiTenantDatabaseInitializer>();
            foreach (var initializer in initializers)
            {
                initializer.InitializeDatabasesAsync(CancellationToken.None).GetAwaiter().GetResult();
            }

            app.UseMultiTenant();
            app.UseTenantMiddleware();

            return app;
        }

        public static FinbuckleMultiTenantBuilder<Tenant> WithFakeStrategy(this FinbuckleMultiTenantBuilder<Tenant> builder) =>
            builder.WithStaticStrategy(MultitenancyConstants.Root.Key);

        public static FinbuckleMultiTenantBuilder<Tenant> WithQueryStringStrategy(this FinbuckleMultiTenantBuilder<Tenant> builder, string queryStringKey) =>
            builder.WithDelegateStrategy(context =>
            {
                if (context is HttpContext httpContext)
                {
                    httpContext.Request.Query.TryGetValue(queryStringKey, out StringValues tenantIdParam);

                    return Task.FromResult((string?)tenantIdParam.ToString());
                }

                return Task.FromResult((string?)null);
            });

        public static FinbuckleMultiTenantBuilder<Tenant> WithOriginOrRefererStrategy(this FinbuckleMultiTenantBuilder<Tenant> builder, string queryStringKey) =>
            builder.WithDelegateStrategy(context =>
            {
                if (context is HttpContext httpContext)
                {
                    return Task.FromResult(ResolveFromOriginOrReferer(httpContext));
                }

                return Task.FromResult((string?)null);
            });


        private static string ResolveFromOriginOrReferer(HttpContext context)
        {
            var origins = context.Request.Headers["origin"];
            if (origins.Count > 0 && origins[0] != "null")
                return ExtractTenantFromUrl(origins[0]);

            var referers = context.Request.Headers["referer"];
            if (referers.Count > 0 && referers[0] != "null")
                return ExtractTenantFromUrl(referers[0]);

            return null;
        }

        static string ExtractTenantFromUrl(string origin)
        {
            if (origin.Contains("http://"))
                origin = origin.Replace("http://", "");

            if (origin.Contains("https://"))
                origin = origin.Replace("https://", "");

            if (origin.StartsWith("localhost"))
                return null;

            var domainParser = new DomainParser(new WebTldRuleProvider());
            var domainInfo = domainParser.Parse(origin);
            var tenantId = domainInfo.SubDomain ?? domainInfo.Domain;
            return tenantId;
        }
    }
}
