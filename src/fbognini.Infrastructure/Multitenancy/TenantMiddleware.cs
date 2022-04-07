
using fbognini.Application.Multitenancy;
using fbognini.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Multitenancy;

public class TenantMiddleware : IMiddleware
{
    private readonly ITenantService _tenantService;

    public TenantMiddleware(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!ExcludePath(context))
        {
            string tenantId = TenantResolver.Resolver(context);
            if (!string.IsNullOrEmpty(tenantId))
            {
                _tenantService.SetCurrentTenant(tenantId);
            }
            else
            {
                throw new IdentityException("Authentication failed");
            }
        }

        await next(context);
    }

    private bool ExcludePath(HttpContext context)
    {
        var listExclude = new List<string>()
            {
                "/swagger",
                "/jobs"
            };

        foreach (string item in listExclude)
        {
            if (context.Request.Path.StartsWithSegments(item))
            {
                return true;
            }
        }

        return false;
    }
}