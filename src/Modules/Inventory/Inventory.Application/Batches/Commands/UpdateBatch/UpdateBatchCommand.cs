using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Inventory.Contracts.Dtos;

namespace Invoria.Inventory.Application.Batches.Commands.UpdateBatch;

public class UpdateBatchCommand : ICommand<BatchDto>
{
    public string Id { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }

    public UpdateBatchCommand(string id, int quantity, decimal purchasePrice)
    {
        Id = id;
        Quantity = quantity;
        PurchasePrice = purchasePrice;
    }
}
