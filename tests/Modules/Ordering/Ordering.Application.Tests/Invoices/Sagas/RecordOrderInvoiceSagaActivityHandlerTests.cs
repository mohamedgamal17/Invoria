using Invoria.Ordering.Application.Invoices.Sagas.Activities;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderInvoice;
using MediatR;
using Moq;

namespace Invoria.Ordering.Application.Tests.Invoices.Sagas;

[TestFixture]
public class RecordOrderInvoiceSagaActivityHandlerTests
{
    [Test]
    public async Task Sends_RecordOrderInvoiceCommand_from_activity()
    {
        var mediator = new Mock<IMediator>();

        var handler = new RecordOrderInvoiceSagaActivityHandler(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RecordOrderInvoiceSagaActivityHandler>>());

        await handler.Handle(new RecordOrderInvoiceSagaActivity("order-1", "invoice-1"));

        mediator.Verify(
            m => m.Send(
                It.Is<RecordOrderInvoiceCommand>(c =>
                    c.OrderId == "order-1" &&
                    c.InvoiceId == "invoice-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
