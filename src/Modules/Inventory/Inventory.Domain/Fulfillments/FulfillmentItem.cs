using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Inventory.Domain.Fulfillments;

public class FulfillmentItem : AuditedEntity
{
    public string FulfillmentId { get; private set; } = null!;

    public string ProductId { get; private set; } = null!;

    public string AllocationItemId { get; private set; } = null!;

    public int AllocatedQuantity { get; private set; }

    private FulfillmentItem()
    {
    }

    public FulfillmentItem(
        string id,
        string fulfillmentId,
        string productId,
        string allocationItemId,
        int allocatedQuantity)
    {
        Guard.Against.NullOrWhiteSpace(id);
        Guard.Against.OutOfRange(id.Length, nameof(id), 1, FulfillmentItemTableConsts.IdMaxLength);
        Guard.Against.NullOrWhiteSpace(fulfillmentId);
        Guard.Against.OutOfRange(fulfillmentId.Length, nameof(fulfillmentId), 1, FulfillmentItemTableConsts.FulfillmentIdMaxLength);
        Guard.Against.NullOrWhiteSpace(productId);
        Guard.Against.OutOfRange(productId.Length, nameof(productId), 1, FulfillmentItemTableConsts.ProductIdMaxLength);
        Guard.Against.NullOrWhiteSpace(allocationItemId);
        Guard.Against.OutOfRange(allocationItemId.Length, nameof(allocationItemId), 1, FulfillmentItemTableConsts.AllocationItemIdMaxLength);
        Guard.Against.NegativeOrZero(allocatedQuantity);

        Id = id;
        FulfillmentId = fulfillmentId;
        ProductId = productId;
        AllocationItemId = allocationItemId;
        AllocatedQuantity = allocatedQuantity;
    }
}
