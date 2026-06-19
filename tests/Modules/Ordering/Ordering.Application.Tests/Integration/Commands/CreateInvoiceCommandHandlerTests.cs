using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Ordering.Application.Invoices.Commands.CreateInvoice;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.CompleteOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderInvoice;
using Invoria.Ordering.Domain.Invoices;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Integration.Commands;

[TestFixture]
public class CreateInvoiceCommandHandlerTests : OrderTestFixture
{
    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearDataAsync();
        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Invocations.Clear();
    }

    private async Task ClearDataAsync()
    {
        var db = Scope.Resolve<OrderingDbContext>();
        var invoices = await db.Set<Invoice>().ToListAsync();
        db.RemoveRange(invoices);
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_create_invoice_for_completed_order_without_returns()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new CompleteOrderCommand(order.Id));

        var result = await Mediator.Send(new CreateInvoiceCommand(order.Id));

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.InvoiceNumber.Should().NotBeNullOrEmpty();
        result.Value.OrderId.Should().Be(order.Id);
        result.Value.Items.Should().NotBeEmpty();
        result.Value.TotalPrice.Should().Be(result.Value.Items.Sum(i => i.LineTotal));
        result.Value.Subtotal.Should().Be(result.Value.TotalPrice);

        var persisted = await Scope.Resolve<OrderingDbContext>()
            .Set<Invoice>()
            .Include(i => i.Items)
            .SingleAsync(i => i.OrderId == order.Id);

        persisted.Items.Should().HaveCount(result.Value.Items.Count);

        var orderFromDb = await Scope.Resolve<OrderingDbContext>()
            .Set<Order>()
            .AsNoTracking()
            .SingleAsync(o => o.Id == order.Id);

        orderFromDb.InvoiceId.Should().BeNull();
    }

    [Test]
    public async Task Should_deduct_returned_quantities_on_invoice_lines()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var line = await Scope.Resolve<OrderingDbContext>()
            .Set<Order>()
            .Where(o => o.Id == order.Id)
            .SelectMany(o => o.Items)
            .Select(i => new { i.Id, i.Quantity, i.Price })
            .FirstAsync();

        await Mediator.Send(
            new CompleteOrderCommand(order.Id, [new CompleteReturnItemLine(line.Id, 1)]));

        var result = await Mediator.Send(new CreateInvoiceCommand(order.Id));

        result.ShouldBeSuccess();
        var invoiceLine = result.Value!.Items.Single(i => i.OrderItemId == line.Id);
        invoiceLine.Quantity.Should().Be(line.Quantity - 1);
        invoiceLine.LineTotal.Should().Be(line.Price * (line.Quantity - 1));
        result.Value.TotalPrice.Should().Be(result.Value.Items.Sum(i => i.LineTotal));
    }

    [Test]
    public async Task Should_set_order_invoice_id_via_RecordOrderInvoice()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new CompleteOrderCommand(order.Id));

        var createResult = await Mediator.Send(new CreateInvoiceCommand(order.Id));
        createResult.ShouldBeSuccess();

        var recordResult = await Mediator.Send(
            new RecordOrderInvoiceCommand(order.Id, createResult.Value!.Id));
        recordResult.ShouldBeSuccess();

        var orderFromDb = await Scope.Resolve<OrderingDbContext>()
            .Set<Order>()
            .AsNoTracking()
            .SingleAsync(o => o.Id == order.Id);

        orderFromDb.InvoiceId.Should().Be(createResult.Value.Id);
    }
}
