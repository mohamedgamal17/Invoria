using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Ordering.Application.Invoices.Commands.CreateInvoice;
using Invoria.Ordering.Application.Invoices.Queries.ListInvoices;
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

using Invoria.Ordering.Application.Tests.Orders;

namespace Invoria.Ordering.Application.Tests.Invoices.Queries;

[TestFixture]
public class ListInvoicesQueryHandlerTests : OrderTestFixture
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

    private async Task<(Order Order, string InvoiceId)> CreateCompletedOrderWithInvoiceAsync()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new CompleteOrderCommand(order.Id));

        var createResult = await Mediator.Send(new CreateInvoiceCommand(order.Id));
        createResult.ShouldBeSuccess();

        return (order, createResult.Value!.Id);
    }

    [Test]
    public async Task Should_return_empty_page_when_no_invoices()
    {
        var query = new ListInvoicesQuery { Skip = 0, Length = 10 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.AssertPagingDto(0, 10, 0, 0);
    }

    [Test]
    public async Task Should_return_paged_invoices_ordered_by_id_descending()
    {
        var (orderA, invoiceA) = await CreateCompletedOrderWithInvoiceAsync();
        var (orderB, invoiceB) = await CreateCompletedOrderWithInvoiceAsync();
        var (orderC, invoiceC) = await CreateCompletedOrderWithInvoiceAsync();

        var query = new ListInvoicesQuery { Skip = 1, Length = 2 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(1, 2, 3, 2);

        var orderedIds = new[] { invoiceA, invoiceB, invoiceC }.OrderByDescending(x => x).ToList();
        page.Data.Select(x => x.Id).Should().Equal(orderedIds.Skip(1).Take(2));
        page.Data.Should().OnlyContain(i =>
            i.OrderId == orderA.Id || i.OrderId == orderB.Id || i.OrderId == orderC.Id);
    }

    [Test]
    public async Task Should_filter_by_customer_id()
    {
        var (matchingOrder, matchingInvoiceId) = await CreateCompletedOrderWithInvoiceAsync();
        await CreateCompletedOrderWithInvoiceAsync();

        var query = new ListInvoicesQuery
        {
            Skip = 0,
            Length = 10,
            CustomerId = matchingOrder.CustomerId
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(matchingInvoiceId);
        page.Data.Single().CustomerId.Should().Be(matchingOrder.CustomerId);
    }

    [Test]
    public async Task Should_filter_by_order_id()
    {
        var (order, invoiceId) = await CreateCompletedOrderWithInvoiceAsync();
        await CreateCompletedOrderWithInvoiceAsync();

        var query = new ListInvoicesQuery
        {
            Skip = 0,
            Length = 10,
            OrderId = order.Id
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(invoiceId);
        page.Data.Single().OrderId.Should().Be(order.Id);
        page.Data.Single().Items.Should().NotBeEmpty();
    }
}
