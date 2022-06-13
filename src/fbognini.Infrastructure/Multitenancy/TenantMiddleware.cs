
using fbognini.Application.Multitenancy;
using fbognini.Core.Exceptions;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Multitenancy
{

    public class TenantMiddleware : IMiddleware
    {
        private readonly ITenantInfo tenantInfo;
        private readonly MultitenancySettings multitenancySettings;

        public TenantMiddleware(ITenantInfo tenantInfo, IOptions<MultitenancySettings> options)
        {
            this.tenantInfo = tenantInfo;
            this.multitenancySettings = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if ((multitenancySettings.IncludeAll && (StartWithPaths(context, multitenancySettings.IncludePaths) || !StartWithPaths(context, multitenancySettings.ExcludePaths)))
                || StartWithPaths(context, multitenancySettings.IncludePaths))
            {
                if (tenantInfo == null)
                {
                    throw new IdentityException("Authentication failed");
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