using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Inventory.Domain.Returns;

public class ReturnLine : AuditedEntity
{
    public string ReturnId { get; private set; } = null!;

    public string OrderItemId { get; private set; } = null!;

    public string ProductId { get; private set; } = null!;

    public int Quantity { get; private set; }

    private ReturnLine()
    {
    }

    public static ReturnLine Create(string orderItemId, string productId, int quantity) =>
        new(Guid.NewGuid().ToString(), orderItemId, productId, quantity);

    public ReturnLine(
        string id,
        string returnId,
        string orderItemId,
        string productId,
        int quantity)
    {
        Guard.Against.NullOrWhiteSpace(id);
        Guard.Against.OutOfRange(id.Length, nameof(id), 1, ReturnLineTableConsts.IdMaxLength);
        Guard.Against.NullOrWhiteSpace(returnId);
        Guard.Against.OutOfRange(returnId.Length, nameof(returnId), 1, ReturnLineTableConsts.ReturnIdMaxLength);
        ValidateLineValues(orderItemId, productId, quantity);

        Id = id;
        ReturnId = returnId;
        OrderItemId = orderItemId;
        ProductId = productId;
        Quantity = quantity;
    }

    private ReturnLine(
        string id,
        string orderItemId,
        string productId,
        int quantity)
    {
        Guard.Against.NullOrWhiteSpace(id);
        Guard.Against.OutOfRange(id.Length, nameof(id), 1, ReturnLineTableConsts.IdMaxLength);
        ValidateLineValues(orderItemId, productId, quantity);

        Id = id;
        OrderItemId = orderItemId;
        ProductId = productId;
        Quantity = quantity;
    }

    internal void AttachToReturn(string returnId)
    {
        Guard.Against.NullOrWhiteSpace(returnId);
        Guard.Against.OutOfRange(returnId.Length, nameof(returnId), 1, ReturnLineTableConsts.ReturnIdMaxLength);

        if (!string.IsNullOrWhiteSpace(ReturnId))
        {
            throw new InvalidOperationException("Return line is already attached to a return.");
        }

        ReturnId = returnId;
    }

    private static void ValidateLineValues(string orderItemId, string productId, int quantity)
    {
        Guard.Against.NullOrWhiteSpace(orderItemId);
        Guard.Against.OutOfRange(orderItemId.Length, nameof(orderItemId), 1, ReturnLineTableConsts.OrderItemIdMaxLength);
        Guard.Against.NullOrWhiteSpace(productId);
        Guard.Against.OutOfRange(productId.Length, nameof(productId), 1, ReturnLineTableConsts.ProductIdMaxLength);
        Guard.Against.NegativeOrZero(quantity);
    }
}
