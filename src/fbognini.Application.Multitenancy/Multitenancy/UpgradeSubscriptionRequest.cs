using fbognini.FluentValidation;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Application.Multitenancy
{

    public class UpgradeSubscriptionRequest : IRequest<string>
    {
        public string TenantId { get; set; } = default!;
        public DateTime ExtendedExpiryDate { get; set; }
    }

    public class UpgradeSubscriptionRequestValidator : CustomValidator<UpgradeSubscriptionRequest>
    {
        public UpgradeSubscriptionRequestValidator() =>
            RuleFor(t => t.TenantId)
                .NotEmpty();
    }

    public class UpgradeSubscriptionRequestHandler : IRequestHandler<UpgradeSubscriptionRequest, string>
    {
        private readonly ITenantService tenantService;

        public UpgradeSubscriptionRequestHandler(ITenantService tenantService) => this.tenantService = tenantService;

        public Task<string> Handle(UpgradeSubscriptionRequest request, CancellationToken cancellationToken) =>
            tenantService.UpdateSubscription(request.TenantId, request.ExtendedExpiryDate);
    }

}