using fbognini.Application.Persistence;
using fbognini.FluentValidation;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace fbognini.Application.Multitenancy;

public class CreateTenantRequestValidator : CustomValidator<CreateTenantRequest>
{
    public CreateTenantRequestValidator(
        ITenantService tenantService,
        //IStringLocalizer<CreateTenantRequestValidator> T,
        IConnectionStringValidator connectionStringValidator)
    {
        RuleFor(t => t.Identifier).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (id, _) => !await tenantService.ExistsWithIdAsync(id))
                //.WithMessage((_, id) => T["Tenant {0} already exists.", id]);
                .WithMessage((_, id) => string.Format("Tenant {0} already exists.", id));

        RuleFor(t => t.Name).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (name, _) => !await tenantService.ExistsWithNameAsync(name!))
                //.WithMessage((_, name) => T["Tenant {0} already exists.", name]);
                .WithMessage((_, name) => string.Format("Tenant {0} already exists.", name));

        RuleFor(t => t.ConnectionString).Cascade(CascadeMode.Stop)
            .Must((_, cs) => string.IsNullOrWhiteSpace(cs) || connectionStringValidator.TryValidate(cs))
                //.WithMessage(T["Connection string invalid."]);
                .WithMessage(string.Format("Connection string invalid."));

        RuleFor(t => t.AdminEmail).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();
    }
}