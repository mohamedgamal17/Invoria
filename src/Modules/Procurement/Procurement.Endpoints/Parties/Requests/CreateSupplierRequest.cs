using FluentValidation;

namespace Invoria.Procurement.Endpoints.Parties.Requests;

public sealed class CreateSupplierRequest : SupplierRequest
{
}

public sealed class CreateSupplierRequestValidator : SupplierRequestValidator<CreateSupplierRequest>
{
}

