using System.Reflection;
using FluentAssertions;
using Invoria.Ordering.Application.Orders.Sagas;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;
using Rebus.Sagas;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class OrderSagaTests
{
    [Test]
    public async Task Handle_new_saga_populates_state_from_integration_event()
    {
        var saga = new OrderSaga();
        MountSaga(saga, new OrderSagaState(), isNew: true);

        var message = new OrderCreatedIntegrationEvent
        {
            OccurredOn = DateTimeOffset.UtcNow,
            Order = new OrderModel
            {
                Id = "order-1",
                OrderNumber = "ON-1",
                CustomerId = "cust-1",
                OrderStatus = OrderStatus.Pending,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 100m,
                AmountPaid = 0m,
                AmountOutstanding = 100m,
                Lines = []
            }
        };

        await saga.Handle(message);

        saga.Data.OrderId.Should().Be("order-1");
        saga.Data.OrderNumber.Should().Be("ON-1");
        saga.Data.CustomerId.Should().Be("cust-1");
        saga.Data.State.Should().Be(OrderSagaProcessState.Created);
    }

    [Test]
    public async Task Handle_existing_saga_is_idempotent()
    {
        var saga = new OrderSaga();
        var existing = new OrderSagaState
        {
            Id = Guid.NewGuid(),
            OrderId = "order-1",
            OrderNumber = "ON-ORIGINAL",
            CustomerId = "cust-original",
            State = OrderSagaProcessState.Created
        };
        MountSaga(saga, existing, isNew: false);

        var message = new OrderCreatedIntegrationEvent
        {
            OccurredOn = DateTimeOffset.UtcNow,
            Order = new OrderModel
            {
                Id = "order-1",
                OrderNumber = "ON-DUPLICATE",
                CustomerId = "cust-duplicate",
                OrderStatus = OrderStatus.Pending,
                PaymentType = OrderPaymentType.Debt,
                PaymentStatus = OrderPaymentStatus.Unpaid,
                TotalOrderAmount = 50m,
                AmountPaid = 0m,
                AmountOutstanding = 50m,
                Lines = []
            }
        };

        await saga.Handle(message);

        saga.Data.OrderNumber.Should().Be("ON-ORIGINAL");
        saga.Data.CustomerId.Should().Be("cust-original");
    }

    private static void MountSaga(OrderSaga saga, OrderSagaState data, bool isNew)
    {
        saga.Data = data;

        typeof(Saga).GetProperty(
                "HoldsNewSagaDataInstance",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(saga, isNew);
    }
}
