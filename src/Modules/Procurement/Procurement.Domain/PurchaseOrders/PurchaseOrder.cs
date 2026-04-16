using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Procurement.Domain.PurchaseOrders;

public class PurchaseOrder : AuditedAggregateRoot
{
    private readonly List<PurchaseOrderItem> _items = new();
    private readonly List<PurchaseStateHistory> _stateHistory = new();

    public string PurchaseNumber { get; private set; } = null!;

    public string SupplierId { get; private set; } = null!;

    public PurchaseState State { get; private set; }

    public DateTime? OrderDate { get; private set; }

    public DateTime? ExpectedDeliveryDate { get; private set; }

    public DateTime? CompletedDate { get; private set; }

    public decimal SubTotal { get; private set; }

    public decimal TaxAmount { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public decimal TotalAmount { get; private set; }

    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    public IReadOnlyCollection<PurchaseStateHistory> StateHistory => _stateHistory.AsReadOnly();

    private PurchaseOrder()
    {
    }

    public PurchaseOrder(
        string id,
        string purchaseNumber,
        string supplierId,
        DateTime? orderDate,
        DateTime? expectedDeliveryDate,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(purchaseNumber))
        {
            throw new ArgumentException(
                "Purchase number cannot be empty.",
                nameof(purchaseNumber));
        }

        if (string.IsNullOrWhiteSpace(supplierId))
        {
            throw new ArgumentException("Supplier id cannot be empty.", nameof(supplierId));
        }

        Id = id;
        PurchaseNumber = purchaseNumber;
        SupplierId = supplierId;
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        State = PurchaseState.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
        RecalculateFinancials();
    }

    public void SetHeaderFinancials(decimal taxAmount, decimal discountAmount)
    {
        EnsureState(PurchaseState.Draft, "Header financials can only be set in Draft.");

        if (taxAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taxAmount), "Tax cannot be negative.");
        }

        if (discountAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(discountAmount), "Discount cannot be negative.");
        }

        TaxAmount = taxAmount;
        DiscountAmount = discountAmount;
        RecalculateFinancials();
    }

    public void AddItem(PurchaseOrderItem item)
    {
        EnsureState(PurchaseState.Draft, "Lines can only be added in Draft.");

        if (item.PurchaseOrderId != Id)
        {
            throw new InvalidOperationException("Item does not belong to this purchase order.");
        }

        _items.Add(item);
        RecalculateFinancials();
    }

    public void Submit()
    {
        ApplyTransition(PurchaseState.Submitted, null);
    }

    public void Approve()
    {
        ApplyTransition(PurchaseState.Approved, null);
    }

    public void Complete()
    {
        EnsureState(PurchaseState.Approved, "Only an approved purchase order can be completed.");

        if (!HasValidLinesForCompletion())
        {
            throw new InvalidOperationException(
                "Cannot complete: every line must have positive quantity and non-negative unit price.");
        }

        ApplyTransition(PurchaseState.Completed, null);
        CompletedDate = DateTime.UtcNow.Date;
    }

    public void Cancel(string? reason)
    {
        ApplyTransition(PurchaseState.Cancelled, reason);
    }

    public void Reject(string? reason)
    {
        ApplyTransition(PurchaseState.Rejected, reason);
    }

    public void RecalculateFinancials()
    {
        SubTotal = _items.Sum(i => i.LineTotal);
        TotalAmount = SubTotal - DiscountAmount + TaxAmount;
    }

    private bool HasValidLinesForCompletion()
    {
        if (_items.Count == 0)
        {
            return false;
        }

        return _items.All(i => i.Quantity > 0 && i.UnitPrice >= 0);
    }

    private void ApplyTransition(PurchaseState next, string? reason)
    {
        if (!PurchaseOrderStateTransitionRules.CanTransition(State, next))
        {
            throw new InvalidOperationException(
                $"Cannot transition from {State} to {next}.");
        }

        var from = State;
        State = next;
        AppendStateHistory(from, next, reason);
    }

    private void AppendStateHistory(
        PurchaseState from,
        PurchaseState to,
        string? reason)
    {
        _stateHistory.Add(new PurchaseStateHistory(
            Guid.NewGuid().ToString("N"),
            Id,
            from,
            to,
            DateTimeOffset.UtcNow,
            reason));
    }

    private void EnsureState(PurchaseState expected, string message)
    {
        if (State != expected)
        {
            throw new InvalidOperationException(message);
        }
    }
}
