using FluentValidation;

namespace Invoria.Inventory.Endpoints.Batches.Requests;

public class UpdateBatchRequest
{
    public string Id { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
}

public class UpdateBatchRequestValidator : AbstractValidator<UpdateBatchRequest>
{
    public UpdateBatchRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PurchasePrice).GreaterThan(0);
    }
}
