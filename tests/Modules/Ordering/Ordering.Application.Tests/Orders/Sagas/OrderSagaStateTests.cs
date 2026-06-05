using FluentAssertions;
using Invoria.Inventory.Contracts.Allocations.Enums;
using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.Ordering.Application.Orders.Sagas;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class OrderSagaStateTests
{
    [Test]
    public void ApplyAccepted_sets_request_allocation_state()
    {
        var state = CreateState(OrderSagaProcessState.Created);

        state.ApplyAccepted(CreateOrder("ON-2", "cust-2"));

        state.State.Should().Be(OrderSagaProcessState.RequestAllocation);
        state.OrderNumber.Should().Be("ON-2");
        state.CustomerId.Should().Be("cust-2");
    }

    [Test]
    public void ApplyAllocationCreated_sets_allocate_state_and_allocation_id()
    {
        var state = CreateState(OrderSagaProcessState.RequestAllocation);

        state.ApplyAllocationCreated(CreateAllocation("alloc-9"));

        state.State.Should().Be(OrderSagaProcessState.Allocate);
        state.AllocationId.Should().Be("alloc-9");
    }

    private static OrderSagaState CreateState(string sagaState) =>
        new()
        {
            OrderId = "order-1",
            OrderNumber = "ON-1",
            CustomerId = "cust-1",
            State = sagaState
        };

    private static OrderModel CreateOrder(string orderNumber, string customerId) =>
        new()
        {
            Id = "order-1",
            OrderNumber = orderNumber,
            CustomerId = customerId,
            OrderStatus = OrderStatus.Processing,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            TotalOrderAmount = 100m,
            AmountPaid = 0m,
            AmountOutstanding = 100m,
            Lines = []
        };

    private static AllocationModel CreateAllocation(string allocationId) =>
        new()
        {
            Id = allocationId,
            OrderId = "order-1",
            Status = AllocationStatus.Pending,
            Lines = []
        };
}
