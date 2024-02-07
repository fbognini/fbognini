using fbognini.Core.Exceptions;
using fbognini.Infrastructure.Entities;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Multitenancy
{
    public class TenantGuardMiddleware : IMiddleware
    {
        private readonly Tenant? tenant;
        private readonly MultitenancySettings multitenancySettings;

        public TenantGuardMiddleware(ITenantInfo tenant, IOptions<MultitenancySettings> options)
        {
            this.tenant = tenant as Tenant;
            this.multitenancySettings = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if ((multitenancySettings.IncludeAll && (StartWithPaths(context, multitenancySettings.IncludePaths) || !TenantGuardMiddleware.StartWithPaths(context, multitenancySettings.ExcludePaths)))
                || StartWithPaths(context, multitenancySettings.IncludePaths))
            {
                if (tenant == null)
                {
                    throw new IdentityException("Tenant is not provided or it doesn't exist.", "Tenant authentication failed.");
                }

                if (tenant.Identifier != MultitenancyConstants.Root.Key)
                {
                    if (!tenant.IsActive)
                    {
                        throw new IdentityException("Tenant is not active. Please contact the Administrator.", "Tenant is not active.");
                    }

                    if (DateTime.UtcNow > tenant.ValidUpto)
                    {
                        throw new IdentityException($"Tenant validity has expired on {tenant.ValidUpto:O}. Please contact the Administrator.", "Tenant validity has expired.");
                    }
                }
            }

            await next(context);
        }

        private static bool StartWithPaths(HttpContext context, List<string> paths)
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