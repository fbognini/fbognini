using fbognini.Application.Entities;
using fbognini.Application.Multitenancy;
using fbognini.Infrastructure.Persistence;
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
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Multitenancy
{
    public static class Startup
    {
        public static FinbuckleMultiTenantBuilder<TTenant> AddMultitenancy<TTenant>(this IServiceCollection services, IConfiguration configuration)
            where TTenant : Tenant, new()

        {
            // TODO: We should probably add specific dbprovider/connectionstring setting for the tenantDb with a fallback to the main databasesettings
            var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            string rootConnectionString = databaseSettings.ConnectionString;
            if (string.IsNullOrEmpty(rootConnectionString)) throw new InvalidOperationException("DB ConnectionString is not configured.");
            string dbProvider = databaseSettings.DBProvider;
            if (string.IsNullOrEmpty(dbProvider)) throw new InvalidOperationException("DB Provider is not configured.");

            return services
                .Configure<MultitenancySettings>(configuration.GetSection(nameof(MultitenancySettings)))
                //.AddDbContext<TenantDbContext>(m => m.UseDatabase(dbProvider, rootConnectionString))
                .AddDbContext<TenantDbContext<TTenant>>(m => m.UseSqlServer(rootConnectionString))
                .AddScoped<ITenantService, TenantService<TTenant>>()
                .AddTenantMiddleware()
                .AddMultiTenant<TTenant>()
                    .WithEFCoreStore<TenantDbContext<TTenant>, TTenant>()
                    ;
        }

        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app) =>
            app.UseMultiTenant();

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
