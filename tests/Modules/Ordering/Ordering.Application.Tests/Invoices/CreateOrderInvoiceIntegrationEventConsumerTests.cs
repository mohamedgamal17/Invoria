using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Invoices.Commands.CreateInvoice;
using Invoria.Ordering.Application.Invoices.Consumers;
using Invoria.Ordering.Contracts.Invoices.Dtos;
using Invoria.Ordering.Contracts.Invoices.Events;
using MediatR;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Invoices;

[TestFixture]
public class CreateOrderInvoiceIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_CreateInvoiceCommand_and_publishes_OrderInvoiceCreatedIntegrationEvent()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new InvoiceDto { Id = "inv-1", OrderId = "order-1" }));

        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var consumer = new CreateOrderInvoiceIntegrationEventConsumer(
            mediator.Object,
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CreateOrderInvoiceIntegrationEventConsumer>>());

        var message = new CreateOrderInvoiceIntegrationEvent
        {
            OrderId = "order-1"
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<CreateInvoiceCommand>(c => c.OrderId == "order-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderInvoiceCreatedIntegrationEvent>(e =>
                    e.OrderId == "order-1" &&
                    e.InvoiceId == "inv-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
}
