using fbognini.FluentValidation;
using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Application.Multitenancy
{

    public class GetTenantRequest : IRequest<TenantDto>
    {
        public string TenantId { get; set; } = default!;

        public GetTenantRequest(string tenantId) => TenantId = tenantId;
    }

    public class GetTenantRequestValidator : CustomValidator<GetTenantRequest>
    {
        public GetTenantRequestValidator() =>
            RuleFor(t => t.TenantId)
                .NotEmpty();
    }

    public class GetTenantRequestHandler : IRequestHandler<GetTenantRequest, TenantDto>
    {
        private readonly ITenantService tenantService;

        public GetTenantRequestHandler(ITenantService tenantService) => this.tenantService = tenantService;

        public Task<TenantDto> Handle(GetTenantRequest request, CancellationToken cancellationToken) =>
            tenantService.GetByIdAsync(request.TenantId);
    }

}