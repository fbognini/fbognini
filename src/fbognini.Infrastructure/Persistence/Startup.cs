﻿using fbognini.Application.Entities;
using fbognini.Application.Multitenancy;
using fbognini.Application.Persistence;
using fbognini.Infrastructure.Common;
using fbognini.Infrastructure.Multitenancy;
using fbognini.Infrastructure.Persistence;
using fbognini.Infrastructure.Persistence.ConnectionString;
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
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence
{
    public static class Startup
    {
        public static IServiceCollection AddPersistence<T>(this IServiceCollection services, IConfiguration configuration)
            where T: DbContext
        {
            var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();

            return services
                .Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)))
                .AddDbContext<T>(options => options
                    .UseSqlServer(databaseSettings.ConnectionString))

                .AddTransient<IMultiTenantDatabaseInitializer, MultiTenantDatabaseInitializer<T>>()
                .AddTransient<ApplicationDatabaseInitializer<T>>()
                .AddServices(typeof(ICustomSeeder<T>), ServiceLifetime.Transient)
                .AddTransient<ApplicationSeederRunner<T>>()

                .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
                .AddTransient<IConnectionStringValidator, ConnectionStringValidator>()
                ;
        }

        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app) =>
            app.UseMultiTenant();

        private static FinbuckleMultiTenantBuilder<Tenant> WithQueryStringStrategy(this FinbuckleMultiTenantBuilder<Tenant> builder, string queryStringKey) =>
            builder.WithDelegateStrategy(context =>
            {
                if (context is not HttpContext httpContext)
                {
                    return Task.FromResult((string?)null);
                }

                httpContext.Request.Query.TryGetValue(queryStringKey, out StringValues tenantIdParam);

                return Task.FromResult((string?)tenantIdParam.ToString());
            });

        private static FinbuckleMultiTenantBuilder<Tenant> WithOriginOrRefererStrategy(this FinbuckleMultiTenantBuilder<Tenant> builder, string queryStringKey) =>
            builder.WithDelegateStrategy(context =>
            {
                if (context is not HttpContext httpContext)
                {
                    return Task.FromResult((string?)null);
                }

                return Task.FromResult(ResolveFromOriginOrReferer(httpContext));
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