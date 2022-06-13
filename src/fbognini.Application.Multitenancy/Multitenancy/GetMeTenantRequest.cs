using fbognini.FluentValidation;
using Finbuckle.MultiTenant;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Application.Multitenancy
{

    public class GetMeTenantRequest : IRequest<TenantDto>
    {
        public GetMeTenantRequest()
        {

        }
    }


    public class GetMeTenantRequestHandler : IRequestHandler<GetMeTenantRequest, TenantDto>
    {
        private readonly ITenantInfo tenantInfo;
        private readonly ITenantService tenantService;

        public GetMeTenantRequestHandler(ITenantInfo tenantInfo, ITenantService tenantService)
        {
            this.tenantInfo = tenantInfo;
            this.tenantService = tenantService;
        }

        public Task<TenantDto> Handle(GetMeTenantRequest request, CancellationToken cancellationToken) =>
            tenantService.GetByIdAsync(tenantInfo.Id);
    }
}