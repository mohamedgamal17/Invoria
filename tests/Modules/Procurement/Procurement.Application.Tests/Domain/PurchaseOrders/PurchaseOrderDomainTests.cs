using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Contracts.PurchaseOrders;

namespace Invoria.Procurement.Application.Tests.Domain.PurchaseOrders;

[TestFixture]
public class PurchaseOrderDomainTests
{
    [Test]
    public void Constructor_rejects_empty_purchase_number()
    {
        Assert.Throws<ArgumentException>(() =>
            new PurchaseOrder("order-1", "", "supplier-1", null, null, null));
    }

    [Test]
    public void AddItem_only_in_Draft_and_recalculates_SubTotal()
    {
        var order = CreateDraftOrder();
        order.SetHeaderFinancials(10m, 5m);

        var line = NewLine(order.Id, quantity: 2, unitPrice: 100m);
        order.AddItem(line);

        Assert.That(order.SubTotal, Is.EqualTo(200m));
        Assert.That(order.TotalAmount, Is.EqualTo(200m - 5m + 10m));
    }

    [Test]
    public void Submit_Approve_Complete_appends_state_history_and_sets_CompletedDate()
    {
        var order = CreateDraftOrder();
        order.AddItem(NewLine(order.Id, 1, 50m));

        order.Submit();
        order.Approve();
        order.Complete();

        Assert.That(order.State, Is.EqualTo(PurchaseState.Completed));
        Assert.That(order.CompletedDate, Is.Not.Null);
        Assert.That(order.StateHistory.Count, Is.EqualTo(3));
        Assert.That(order.StateHistory.Last().ToState, Is.EqualTo(PurchaseState.Completed));
    }

    [Test]
    public void Complete_throws_when_not_Approved()
    {
        var order = CreateDraftOrder();
        order.AddItem(NewLine(order.Id, 1, 10m));
        order.Submit();

        Assert.Throws<InvalidOperationException>(() => order.Complete());
    }

    [Test]
    public void Complete_throws_when_no_lines()
    {
        var order = CreateDraftOrder();
        order.Submit();
        order.Approve();

        Assert.Throws<InvalidOperationException>(() => order.Complete());
    }

    [Test]
    public void Cancel_from_Draft_is_allowed_and_terminal()
    {
        var order = CreateDraftOrder();
        order.Cancel("no longer needed");

        Assert.That(order.State, Is.EqualTo(PurchaseState.Cancelled));
        Assert.Throws<InvalidOperationException>(() => order.Submit());
    }

    [Test]
    public void PurchaseOrderItem_UnitPrice_is_fixed_after_construction()
    {
        var orderId = NewId();
        var item = NewLine(orderId, 3, 12.5m);

        Assert.That(item.UnitPrice, Is.EqualTo(12.5m));
    }

    [Test]
    public void PurchaseOrderItem_RegisterCreatedBatch_appends_trace_ids()
    {
        var item = NewLine(NewId(), 1, 1m);
        var b1 = NewId();
        var b2 = NewId();

        item.RegisterCreatedBatch(b1);
        item.RegisterCreatedBatch(b2);

        Assert.That(item.CreatedBatchIds, Is.EqualTo(new[] { b1, b2 }));
    }

    [Test]
    public void PurchaseOrderSequence_Create_starts_at_zero_and_Increment_is_sequential()
    {
        var sequence = PurchaseOrderSequence.Create(2026, 4, 7);
        Assert.That(sequence.Id, Is.EqualTo("20260407"));
        Assert.That(sequence.Year, Is.EqualTo(2026));
        Assert.That(sequence.Month, Is.EqualTo(4));
        Assert.That(sequence.Day, Is.EqualTo(7));
        Assert.That(sequence.CurrentValue, Is.EqualTo(0));
        Assert.That(sequence.Increment(), Is.EqualTo(1));
        Assert.That(sequence.Increment(), Is.EqualTo(2));
    }

    private static string NewId() => Guid.NewGuid().ToString("N");

    private static PurchaseOrder CreateDraftOrder()
    {
        return new PurchaseOrder(
            NewId(),
            "2026-00001",
            NewId(),
            DateTime.UtcNow,
            null,
            "creator");
    }

    private static PurchaseOrderItem NewLine(string purchaseOrderId, int quantity, decimal unitPrice)
    {
        return new PurchaseOrderItem(
            NewId(),
            purchaseOrderId,
            NewId(),
            quantity,
            unitPrice,
            supplierProductCode: null);
    }
}
