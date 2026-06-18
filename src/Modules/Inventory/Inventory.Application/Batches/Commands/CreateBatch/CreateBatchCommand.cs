using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Inventory.Contracts.Batches.Dtos;

namespace Invoria.Inventory.Application.Batches.Commands.CreateBatch;

public class CreateBatchCommand : ICommand<BatchDto>
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }

    public CreateBatchCommand(string productId, int quantity, decimal purchasePrice)
    {
        ProductId = productId;
        Quantity = quantity;
        PurchasePrice = purchasePrice;
    }
}
