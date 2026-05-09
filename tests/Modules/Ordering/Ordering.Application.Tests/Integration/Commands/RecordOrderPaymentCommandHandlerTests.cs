using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.CompleteOrder;
using Invoria.Ordering.Application.Orders.Commands.DispatchOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderPayment;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Integration.Commands;

[TestFixture]
public class RecordOrderPaymentCommandHandlerTests : OrderTestFixture
{
    private async Task<Order> PersistOneRandomOrderInNewScopeAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        return (await OrderTestData.PersistRandomOrdersAsync(repo, 1)).Single();
    }

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearOrdersAsync();
        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Invocations.Clear();
    }

    private async Task ClearOrdersAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    private static async Task<decimal> GetTotalOrderAmountFromDbAsync(
        IServiceProvider serviceProvider,
        string orderId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var order = await db.Set<Order>()
            .AsNoTracking()
            .Include(o => o.Items)
            .SingleAsync(o => o.Id == orderId);
        return order.Items.Sum(i => i.Price * i.Quantity);
    }

    private static async Task SetPaymentTypeDebtAsync(IServiceProvider serviceProvider, string orderId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.PaymentType, OrderPaymentType.Debt));
    }

    [Test]
    public async Task Should_record_partial_payment_when_debt_after_completed()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await SetPaymentTypeDebtAsync(ServiceProvider, order.Id);

        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new RecordOrderAllocationSucceededCommand { OrderId = order.Id, CustomerId = order.CustomerId });
        await Mediator.Send(new DispatchOrderCommand(order.Id));
        await Mediator.Send(new CompleteOrderCommand(order.Id));

        var total = await GetTotalOrderAmountFromDbAsync(ServiceProvider, order.Id);
        total.Should().BeGreaterThan(1m);

        var result = await Mediator.Send(
            new RecordOrderPaymentCommand(order.Id, paidAmount: 1m, OrderPaymentMethod.Cash));

        result.ShouldBeSuccess();
        result.Value!.AmountPaid.Should().Be(1m);
        result.Value.AmountOutstanding.Should().Be(total - 1m);
        result.Value.PaymentStatus.Should().Be(OrderPaymentStatus.Partial);
        result.Value.Payments.Should().ContainSingle();
    }

    [Test]
    public async Task Should_record_full_payment_when_immediate_after_completed()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();

        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new RecordOrderAllocationSucceededCommand { OrderId = order.Id, CustomerId = order.CustomerId });
        await Mediator.Send(new DispatchOrderCommand(order.Id));
        await Mediator.Send(new CompleteOrderCommand(order.Id));

        var total = await GetTotalOrderAmountFromDbAsync(ServiceProvider, order.Id);

        var result = await Mediator.Send(
            new RecordOrderPaymentCommand(order.Id, total, OrderPaymentMethod.BankTransfer));

        result.ShouldBeSuccess();
        result.Value!.PaymentStatus.Should().Be(OrderPaymentStatus.Paid);
        result.Value.AmountOutstanding.Should().Be(0);
        result.Value.Payments.Should().ContainSingle();
    }

    [Test]
    public async Task Should_fail_when_order_not_completed()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var result = await Mediator.Send(
            new RecordOrderPaymentCommand(order.Id, 1m, OrderPaymentMethod.Cash));

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var id = Guid.NewGuid().ToString();
        var result = await Mediator.Send(new RecordOrderPaymentCommand(id, 1m, OrderPaymentMethod.Cash));

        result.ShouldBeFailure(typeof(NotFoundException));
    }
}
