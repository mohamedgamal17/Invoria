using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.CompleteOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderPayment;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Orders.Commands;

[TestFixture]
public class RecordOrderPaymentCommandHandlerTests : OrderTestFixture
{
    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearOrdersAsync();
        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Invocations.Clear();
    }

    private async Task ClearOrdersAsync()
    {
        var db = Scope.Resolve<OrderingDbContext>();
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    private async Task<decimal> GetPayableOrderAmountFromDbAsync(string orderId)
    {
        var db = Scope.Resolve<OrderingDbContext>();
        var order = await db.Set<Order>()
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.ReturnItems)
            .SingleAsync(o => o.Id == orderId);
        return order.NetOfTotalOrderAmount;
    }

    private async Task SetPaymentTypeDebtAsync(string orderId)
    {
        var db = Scope.Resolve<OrderingDbContext>();
        await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.PaymentType, OrderPaymentType.Debt));
        db.ChangeTracker.Clear();
    }

    [Test]
    public async Task Should_record_partial_payment_when_debt_after_completed()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();

        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new CompleteOrderCommand(order.Id));
        await SetPaymentTypeDebtAsync(order.Id);

        var payable = await GetPayableOrderAmountFromDbAsync(order.Id);
        payable.Should().BeGreaterThan(1m);

        var result = await Mediator.Send(
            new RecordOrderPaymentCommand(order.Id, paidAmount: 1m, OrderPaymentMethod.Cash));

        result.ShouldBeSuccess();
        result.Value!.AmountPaid.Should().Be(1m);
        result.Value.AmountOutstanding.Should().Be(payable - 1m);
        result.Value.PaymentStatus.Should().Be(OrderPaymentStatus.Partial);
        result.Value.Payments.Should().ContainSingle();
    }

    [Test]
    public async Task Should_record_full_payment_when_immediate_after_completed()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();

        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new CompleteOrderCommand(order.Id));

        var payable = await GetPayableOrderAmountFromDbAsync(order.Id);

        var result = await Mediator.Send(
            new RecordOrderPaymentCommand(order.Id, payable, OrderPaymentMethod.BankTransfer));

        result.ShouldBeSuccess();
        result.Value!.PaymentStatus.Should().Be(OrderPaymentStatus.Paid);
        result.Value.AmountOutstanding.Should().Be(0);
        result.Value.Payments.Should().ContainSingle();
    }

    [Test]
    public async Task Should_fail_when_order_not_completed()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
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
