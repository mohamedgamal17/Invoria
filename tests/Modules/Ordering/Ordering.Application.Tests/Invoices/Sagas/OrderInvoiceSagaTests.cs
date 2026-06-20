using FluentAssertions;
using Invoria.Ordering.Application.Invoices.Sagas;
using Invoria.Ordering.Application.Invoices.Sagas.Activities;
using Invoria.Ordering.Contracts.Invoices.Events;
using Moq;
using Rebus.Bus;
using Rebus.TestHelpers;

namespace Invoria.Ordering.Application.Tests.Invoices.Sagas;

[TestFixture]
public class OrderInvoiceSagaTests
{
    [Test]
    public void Deliver_OrderInvoiceRequestIntegrationEvent_creates_saga_with_requested_state()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderInvoiceSaga(bus.Object));

        fixture.Deliver(BuildOrderInvoiceRequest("order-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderInvoiceSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderInvoiceSagaProcessState.Requested);
        data.InvoiceId.Should().BeNull();

        bus.Verify(
            b => b.Publish(
                It.Is<CreateOrderInvoiceIntegrationEvent>(e => e.OrderId == "order-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public void Deliver_OrderInvoiceCreatedIntegrationEvent_sets_completed_state_and_invoice_id()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderInvoiceSaga(bus.Object));

        fixture.Deliver(BuildOrderInvoiceRequest("order-1"));
        fixture.Deliver(BuildOrderInvoiceCreated("invoice-1", "order-1"));

        fixture.HandlerExceptions.Should().BeEmpty();

        var data = fixture.Data
            .OfType<OrderInvoiceSagaState>()
            .Single(d => d.OrderId == "order-1");

        data.State.Should().Be(OrderInvoiceSagaProcessState.Completed);
        data.InvoiceId.Should().Be("invoice-1");

        bus.Verify(
            b => b.Publish(
                It.Is<RecordOrderInvoiceSagaActivity>(a =>
                    a.OrderId == "order-1" &&
                    a.InvoiceId == "invoice-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public void Deliver_OrderInvoiceCreated_without_saga_does_not_fail()
    {
        var bus = CreateBus();
        using var fixture = SagaFixture.For(() => new OrderInvoiceSaga(bus.Object));

        fixture.Deliver(BuildOrderInvoiceCreated("invoice-orphan", "order-orphan"));

        fixture.HandlerExceptions.Should().BeEmpty();
        fixture.Data.OfType<OrderInvoiceSagaState>().Should().BeEmpty();
    }

    private static Mock<IBus> CreateBus()
    {
        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);
        return bus;
    }

    private static OrderInvoiceRequestIntegrationEvent BuildOrderInvoiceRequest(string orderId) =>
        new() { OrderId = orderId };

    private static OrderInvoiceCreatedIntegrationEvent BuildOrderInvoiceCreated(
        string invoiceId,
        string orderId) =>
        new()
        {
            InvoiceId = invoiceId,
            OrderId = orderId
        };
}
