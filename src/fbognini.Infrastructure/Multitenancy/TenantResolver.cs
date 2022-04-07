using Microsoft.AspNetCore.Http;
using Nager.PublicSuffix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Multitenancy
{
    public static class TenantResolver
    {
        public static string Resolver(HttpContext context)
        {
            string tenantId = ResolveFromUserAuth(context);
            if (!string.IsNullOrEmpty(tenantId))
            {
                return tenantId;
            }

            tenantId = ResolveFromUrl(context);
            if (!string.IsNullOrEmpty(tenantId))
            {
                return tenantId;
            }

            tenantId = ResolveFromHeader(context);
            if (!string.IsNullOrEmpty(tenantId))
            {
                return tenantId;
            }

            tenantId = ResolveFromQuery(context);
            if (!string.IsNullOrEmpty(tenantId))
            {
                return tenantId;
            }

            tenantId = ResolveFromOriginOrReferer(context);
            if (!string.IsNullOrEmpty(tenantId))
            {
                return tenantId;
            }

            return default;
        }

        private static string ResolveFromUserAuth(HttpContext context)
        {
            var principal = context.User;
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var claim = principal.FindFirst("client_tenant");
            return claim?.Value;
        }

        private static string ResolveFromUrl(HttpContext context)
        {
            return ExtractTenantFromUrl(context.Request.Host.Host);
        }

        private static string ResolveFromHeader(HttpContext context)
        {
            context.Request.Headers.TryGetValue("tenant", out var tenantFromHeader);
            return tenantFromHeader;
        }

        private static string ResolveFromQuery(HttpContext context)
        {
            context.Request.Query.TryGetValue("tenant", out var tenantFromQueryString);
            return tenantFromQueryString;
        }

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
