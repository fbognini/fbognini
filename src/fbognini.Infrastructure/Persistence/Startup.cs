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
        public static IServiceCollection AddPersistence<T, TTenantContext, TTenant>(this IServiceCollection services, IConfiguration configuration)
            where T : DbContext
            where TTenantContext : TenantDbContext<TTenant>
            where TTenant : Tenant, new()
        {
            var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();

            return services
                .Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)))
                .AddDbContext<T>(options => options
                    .UseSqlServer(databaseSettings.ConnectionString))

                .AddTransient<IMultiTenantDatabaseInitializer, MultiTenantDatabaseInitializer<T, TTenantContext, TTenant>>()
                .AddTransient<ApplicationDatabaseInitializer<T>>()
                .AddServices(typeof(ICustomSeeder<T>), ServiceLifetime.Transient)
                .AddTransient<ApplicationSeederRunner<T>>()

                .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
                .AddTransient<IConnectionStringValidator, ConnectionStringValidator>()
                ;
        }

        public static IServiceCollection AddPersistence<T, TTenant>(this IServiceCollection services, IConfiguration configuration)
            where T: DbContext
            where TTenant: Tenant, new()
        {
            return services.AddPersistence<T, TenantDbContext<TTenant>, TTenant>(configuration);
        }

        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app) =>
            app.UseMultiTenant();
    }
}
