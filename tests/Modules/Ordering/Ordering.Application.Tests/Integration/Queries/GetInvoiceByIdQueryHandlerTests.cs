using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Invoices.Commands.CreateInvoice;
using Invoria.Ordering.Application.Invoices.Queries.GetInvoiceById;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.CompleteOrder;
using Invoria.Ordering.Application.Tests.Assertions;
using Invoria.Ordering.Domain.Invoices;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Integration.Queries;

[TestFixture]
public class GetInvoiceByIdQueryHandlerTests : OrderTestFixture
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
    public async Task Should_return_invoice_with_line_items_when_found()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new CompleteOrderCommand(order.Id));

        var createResult = await Mediator.Send(new CreateInvoiceCommand(order.Id));
        createResult.ShouldBeSuccess();

        var query = new GetInvoiceByIdQuery { Id = createResult.Value!.Id };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.AssertInvoiceDto(createResult.Value.Id, createResult.Value.InvoiceNumber, order.Id, order.CustomerId);
    }

    [Test]
    public async Task Should_return_failure_when_invoice_not_found()
    {
        var nonExistentId = Guid.NewGuid().ToString();
        var query = new GetInvoiceByIdQuery { Id = nonExistentId };

        var result = await Mediator.Send(query);

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NotFoundException>();
    }
}
