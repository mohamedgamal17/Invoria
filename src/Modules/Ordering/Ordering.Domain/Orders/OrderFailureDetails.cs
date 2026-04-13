using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Ordering.Domain.Orders;

public class OrderFailureDetails : AuditedEntity
{
    public string ItemId { get; private set; }
    public int QuantityRequested { get; private set; }
    public int QuantityAvailable { get; private set; }
    public int Shortage { get; private set; }

    private OrderFailureDetails()
    {
    }

    public OrderFailureDetails(
        string itemId,
        int quantityRequested,
        int quantityAvailable,
        int shortage)
    {
        Guard.Against.NullOrWhiteSpace(itemId);
        Guard.Against.OutOfRange(itemId.Length, nameof(itemId), 1, OrderFailureDetailsTableConsts.ItemIdMaxLength);
        Guard.Against.Negative(quantityRequested);
        Guard.Against.Negative(quantityAvailable);
        Guard.Against.Negative(shortage);

        ItemId = itemId;
        QuantityRequested = quantityRequested;
        QuantityAvailable = quantityAvailable;
        Shortage = shortage;
    }
}
