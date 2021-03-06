using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Application.Multitenancy
{
    public class CreateTenantRequest : IRequest<string>
    {
        public string Identifier { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ConnectionString { get; set; }
        public string AdminEmail { get; set; } = default!;
        public string Issuer { get; set; }
    }

    public class CreateTenantRequestHandler : IRequestHandler<CreateTenantRequest, string>
    {
        private readonly ITenantService tenantService;

        public CreateTenantRequestHandler(ITenantService tenantService) => this.tenantService = tenantService;

        public Task<string> Handle(CreateTenantRequest request, CancellationToken cancellationToken) =>
            tenantService.CreateAsync(request, cancellationToken);
    }

}