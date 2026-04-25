using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;

public sealed class UpdatePurchaseOrderCommand : ICommand<PurchaseOrderDto>
{
    public string Id { get; set; }
    public string SupplierId { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public List<UpdatePurchaseOrderItemCommand> PurchaseOrderItems { get; set; }

    public UpdatePurchaseOrderCommand(
        string id,
        string supplierId,
        decimal taxAmount,
        decimal discountAmount,
        DateTime? orderDate,
        DateTime? expectedDeliveryDate,
        List<UpdatePurchaseOrderItemCommand> purchaseOrderItems)
    {
        Id = id;
        SupplierId = supplierId;
        TaxAmount = taxAmount;
        DiscountAmount = discountAmount;
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        PurchaseOrderItems = purchaseOrderItems;
    }
}

