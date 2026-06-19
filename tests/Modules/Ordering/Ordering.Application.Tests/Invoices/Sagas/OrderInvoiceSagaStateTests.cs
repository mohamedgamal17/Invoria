using FluentAssertions;
using Invoria.Ordering.Application.Invoices.Sagas;

namespace Invoria.Ordering.Application.Tests.Invoices.Sagas;

[TestFixture]
public class OrderInvoiceSagaStateTests
{
    [Test]
    public void ApplyRequested_sets_order_id_and_requested_state()
    {
        var state = new OrderInvoiceSagaState();

        state.ApplyRequested("order-1");

        state.OrderId.Should().Be("order-1");
        state.State.Should().Be(OrderInvoiceSagaProcessState.Requested);
        state.InvoiceId.Should().BeNull();
    }

    [Test]
    public void ApplyCompleted_sets_invoice_id_and_completed_state()
    {
        var state = new OrderInvoiceSagaState
        {
            OrderId = "order-1",
            State = OrderInvoiceSagaProcessState.Requested
        };

        state.ApplyCompleted("invoice-9");

        state.InvoiceId.Should().Be("invoice-9");
        state.State.Should().Be(OrderInvoiceSagaProcessState.Completed);
        state.OrderId.Should().Be("order-1");
    }
}
