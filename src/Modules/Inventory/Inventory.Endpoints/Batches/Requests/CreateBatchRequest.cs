using FluentValidation;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Endpoints.Batches.Requests;

public class CreateBatchRequest
{
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
}

public class CreateBatchRequestValidator : AbstractValidator<CreateBatchRequest>
{
    public CreateBatchRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .MaximumLength(BatchTableConsts.ProductIdMaxLength);

        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.PurchasePrice).GreaterThan(0);
    }
}
