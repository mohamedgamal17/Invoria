using FluentAssertions;
using Invoria.Inventory.Application.Allocations.Commands.ReleaseAllocation;
using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Application.Batches.Commands.AllocateOrder;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class ReleaseAllocationCommandHandlerTests : Batches.BatchTestFixture
{
    [Test]
    public async Task Releases_stock_and_marks_allocation_when_fully_allocated()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        var batchCreated = await Mediator.Send(new CreateBatchCommand(productId, 10, 10m));
        batchCreated.IsSuccess.Should().BeTrue();

        await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = orderItemId, ProductId = productId, Quantity = 4 }])));

        var allocationId = await GetAllocationIdForOrderAsync(orderId);
        (await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId }))
            .IsSuccess.Should().BeTrue();

        var result = await Mediator.Send(new ReleaseAllocationCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var allocation = await db.Set<Allocation>()
            .Include(a => a.Lines)
            .ThenInclude(l => l.BatchAllocations)
            .SingleAsync(a => a.Id == allocationId);

        allocation.Status.Should().Be(AllocationStatus.Released);
        allocation.Lines.Should().ContainSingle();
        allocation.Lines.Single().Status.Should().Be(AllocationLineStatus.Released);
        var batch = await db.Set<Batch>().SingleAsync(b => b.Id == batchCreated.Value!.Id);
        batch.Quantity.Should().Be(10);
        batch.ReservedQuantity.Should().Be(0);

        allocation.Lines.Single().BatchAllocations.Should().ContainSingle();
    }

    [Test]
    public async Task Second_release_is_no_op_when_already_released()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        await Mediator.Send(new CreateBatchCommand(productId, 10, 10m));
        await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = orderItemId, ProductId = productId, Quantity = 2 }])));

        var allocationId = await GetAllocationIdForOrderAsync(orderId);
        await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId });
        await Mediator.Send(new ReleaseAllocationCommand { AllocationId = allocationId });

        var result = await Mediator.Send(new ReleaseAllocationCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocation = await db.Set<Allocation>().SingleAsync(a => a.Id == allocationId);
        allocation.Status.Should().Be(AllocationStatus.Released);
    }

    [Test]
    public async Task Fails_when_allocation_is_pending()
    {
        var orderId = Guid.NewGuid().ToString();
        await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = $"oi-{Guid.NewGuid():N}", ProductId = Guid.NewGuid().ToString(), Quantity = 1 }])));

        var allocationId = await GetAllocationIdForOrderAsync(orderId);

        var result = await Mediator.Send(new ReleaseAllocationCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeFalse();
    }

    private async Task<string> GetAllocationIdForOrderAsync(string orderId)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        return (await db.Set<Allocation>().SingleAsync(a => a.OrderId == orderId)).Id!;
    }

    private static AllocateOrderIntegrationEvent NewAllocateEvent(
        string orderId,
        List<OrderItemModel> items) =>
        new()
        {
            Id = orderId,
            OrderNumber = "ORD-1",
            CustomerId = "cust-1",
            Items = items
        };
}
