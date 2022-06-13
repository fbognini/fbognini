using fbognini.FluentValidation;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Application.Multitenancy
{

    public class DeactivateTenantRequest : IRequest<string>
    {
        public string TenantId { get; set; } = default!;

        public DeactivateTenantRequest(string tenantId) => TenantId = tenantId;
    }

    public class DeactivateTenantRequestValidator : CustomValidator<DeactivateTenantRequest>
    {
        public DeactivateTenantRequestValidator() =>
            RuleFor(t => t.TenantId)
                .NotEmpty();
    }

    public class DeactivateTenantRequestHandler : IRequestHandler<DeactivateTenantRequest, string>
    {
        private readonly ITenantService tenantService;

        public DeactivateTenantRequestHandler(ITenantService tenantService) => this.tenantService = tenantService;

        public Task<string> Handle(DeactivateTenantRequest request, CancellationToken cancellationToken) =>
            tenantService.DeactivateAsync(request.TenantId);
    }

}