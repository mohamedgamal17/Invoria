using FluentValidation;
using Invoria.Procurement.Domain.Parties;

namespace Invoria.Procurement.Endpoints.Parties.Requests;

public sealed class UpdateSupplierRequest : SupplierRequest
{
    public string Id { get; set; } = default!;
}

public sealed class UpdateSupplierRequestValidator : SupplierRequestValidator<UpdateSupplierRequest>
{
    public UpdateSupplierRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(SupplierTableConsts.IdMaxLength);
    }
}

