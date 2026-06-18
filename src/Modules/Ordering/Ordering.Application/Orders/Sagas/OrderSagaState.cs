using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.Ordering.Contracts.Orders.Models;
using Rebus.Sagas;

namespace Invoria.Ordering.Application.Orders.Sagas;

public sealed class OrderSagaState : ISagaData
{
    public Guid Id { get; set; }

    public int Revision { get; set; }

    public string OrderId { get; set; } = default!;

    public string OrderNumber { get; set; } = default!;

    public string CustomerId { get; set; } = default!;

    public string? AllocationId { get; set; }

    public string State { get; set; } = OrderSagaProcessState.Created;

    public void ApplyCreated(OrderModel order)
    {
        OrderId = order.Id;
        OrderNumber = order.OrderNumber;
        CustomerId = order.CustomerId;
        State = OrderSagaProcessState.Created;
    }

    public void ApplyAccepted(OrderModel order)
    {
        OrderNumber = order.OrderNumber;
        CustomerId = order.CustomerId;
        State = OrderSagaProcessState.RequestAllocation;
    }

    public void ApplyAllocationCreated(AllocationModel allocation)
    {
        AllocationId = allocation.Id;
        State = OrderSagaProcessState.Allocate;
    }

    public void ApplyAllocationFailed(string allocationId)
    {
        AllocationId = allocationId;
        State = OrderSagaProcessState.AllocationFailed;
    }

    public void ApplyAllocationSucceeded(string allocationId)
    {
        AllocationId = allocationId;
        State = OrderSagaProcessState.AllocationSucceeded;
    }

    public void ApplyRevisionRequested(string allocationId)
    {
        AllocationId = allocationId;
        State = OrderSagaProcessState.RevisionRequested;
    }

    public void ApplyAllocationReleased(string allocationId)
    {
        AllocationId = allocationId;
        State = OrderSagaProcessState.AllocationReleased;
    }
}
