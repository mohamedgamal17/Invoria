using FluentValidation;
using Invoria.Procurement.Domain.Parties;

namespace Invoria.Procurement.Endpoints.Parties.Requests;

public class SupplierRequest
{
    public string SupplierCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? ContactEmail { get; set; }
    public string? Phone { get; set; }
}

public class SupplierRequestValidator<T> : AbstractValidator<T>
    where T : SupplierRequest
{
    public SupplierRequestValidator()
    {
        RuleFor(x => x.SupplierCode)
            .NotEmpty()
            .MaximumLength(SupplierTableConsts.SupplierCodeMaxLength);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(SupplierTableConsts.NameMaxLength);

        RuleFor(x => x.ContactEmail)
            .MaximumLength(SupplierTableConsts.ContactEmailMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.Phone)
            .MaximumLength(SupplierTableConsts.PhoneMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

