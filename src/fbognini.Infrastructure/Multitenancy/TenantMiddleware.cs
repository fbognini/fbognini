
using fbognini.Application.Entities;
using fbognini.Application.Multitenancy;
using fbognini.Core.Exceptions;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Multitenancy
{

    public class TenantMiddleware : IMiddleware
    {
        private readonly Tenant tenant;
        private readonly MultitenancySettings multitenancySettings;

        public TenantMiddleware(ITenantInfo tenant, IOptions<MultitenancySettings> options)
        {
            this.tenant = tenant as Tenant;
            this.multitenancySettings = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if ((multitenancySettings.IncludeAll && (StartWithPaths(context, multitenancySettings.IncludePaths) || !StartWithPaths(context, multitenancySettings.ExcludePaths)))
                || StartWithPaths(context, multitenancySettings.IncludePaths))
            {
                if (tenant == null)
                {
                    throw new IdentityException("Authentication failed");
                }

                if (tenant.Identifier != MultitenancyConstants.Root.Key)
                {
                    if (!tenant.IsActive)
                    {
                        throw new IdentityException("Tenant is not active. Please contact the Administrator.");
                    }

                    if (DateTime.UtcNow > tenant.ValidUpto)
                    {
                        throw new IdentityException("Tenant validity has expired. Please contact the Administrator.");
                    }
                }
            }

            await next(context);
        }

        private bool StartWithPaths(HttpContext context, List<string> paths)
        {
            if (paths == null)
                return false;

            foreach (string item in paths)
            {
                if (context.Request.Path.StartsWithSegments(item))
                {
                    return true;
                }
            }

            return false;
        }
    }

}