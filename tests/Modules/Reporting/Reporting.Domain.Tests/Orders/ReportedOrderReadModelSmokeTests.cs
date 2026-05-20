using Invoria.Ordering.Contracts.Orders;
using Invoria.Reporting.Domain.Orders;

namespace Invoria.Reporting.Domain.Tests.Orders;

[TestFixture]
public sealed class ReportedOrderReadModelSmokeTests
{
    [Test]
    public void ReportedOrder_graph_assigns_round_trip_scalars_and_children()
    {
        var utc = DateTimeOffset.Parse("2026-05-01T12:00:00Z");
        var order = new ReportedOrder
        {
            Id = "order-1",
            OrderNumber = "ON-9",
            CustomerId = "cust-1",
            OrderStatus = OrderStatus.Completed,
            FullfillmentStatus = FullfillmentStatus.Dispatched,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Partial,
            TotalOrderAmount = 100m,
            AmountPaid = 40m,
            AmountOutstanding = 60m,
            ReplicationVersion = 3,
            SourceLastKnownAt = utc,
            CreatedAt = utc,
            CreatedBy = "repl",
        };

        order.Lines.Add(
            new ReportedOrderLine
            {
                Id = "line-1",
                ReportedOrderId = order.Id,
                ProductId = "p1",
                Quantity = 2,
                UnitPrice = 50m,
                LineTotal = 100m,
                ReportedOrder = order,
            });

        order.Payments.Add(
            new ReportedOrderPayment
            {
                Id = "pay-1",
                ReportedOrderId = order.Id,
                PaidAmount = 40m,
                PaymentMethod = OrderPaymentMethod.Cash,
                PaidAt = utc.AddHours(1),
                CreatedAt = utc,
                ReportedOrder = order,
            });

        Assert.Multiple(() =>
        {
            Assert.That(order.Id, Is.EqualTo("order-1"));
            Assert.That(order.TotalOrderAmount, Is.EqualTo(100m));
            Assert.That(order.Lines, Has.Count.EqualTo(1));
            Assert.That(order.Lines[0].LineTotal, Is.EqualTo(100m));
            Assert.That(order.Payments[0].PaidAmount, Is.EqualTo(40m));
        });
    }
}
