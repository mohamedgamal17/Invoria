using FluentValidation;
using Invoria.Procurement.Domain.Parties;

namespace Invoria.Procurement.Endpoints.Parties.Requests;

public sealed class GetSupplierByIdRequest
{
    public string Id { get; set; } = string.Empty;
}

public sealed class GetSupplierByIdRequestValidator : AbstractValidator<GetSupplierByIdRequest>
{
    public GetSupplierByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(SupplierTableConsts.IdMaxLength);
    }
}
