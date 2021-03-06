using fbognini.FluentValidation;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Application.Multitenancy
{

    public class ActivateTenantRequest : IRequest<string>
    {
        public string TenantId { get; set; } = default!;

        public ActivateTenantRequest(string tenantId) => TenantId = tenantId;
    }

    public class ActivateTenantRequestValidator : CustomValidator<ActivateTenantRequest>
    {
        public ActivateTenantRequestValidator() =>
            RuleFor(t => t.TenantId)
                .NotEmpty();
    }

    public class ActivateTenantRequestHandler : IRequestHandler<ActivateTenantRequest, string>
    {
        private readonly ITenantService tenantService;

        public ActivateTenantRequestHandler(ITenantService tenantService) => this.tenantService = tenantService;

        public Task<string> Handle(ActivateTenantRequest request, CancellationToken cancellationToken) =>
            tenantService.ActivateAsync(request.TenantId);
    }

}