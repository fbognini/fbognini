using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Application.Multitenancy
{

    public class GetAllTenantsRequest : IRequest<List<TenantDto>>
    {
    }

    public class GetAllTenantsRequestHandler : IRequestHandler<GetAllTenantsRequest, List<TenantDto>>
    {
        private readonly ITenantService tenantService;

        public GetAllTenantsRequestHandler(ITenantService tenantService) => this.tenantService = tenantService;

        public Task<List<TenantDto>> Handle(GetAllTenantsRequest request, CancellationToken cancellationToken) =>
            tenantService.GetAllAsync();
    }

}