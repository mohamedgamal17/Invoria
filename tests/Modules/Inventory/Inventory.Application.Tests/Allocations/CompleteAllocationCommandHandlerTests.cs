using FluentAssertions;
using Invoria.Inventory.Application.Allocations.Commands.CompleteAllocation;
using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Application.Allocations.Commands.CreateAllocate;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class CompleteAllocationCommandHandlerTests : Batches.BatchTestFixture
{
    [Test]
    public async Task CompleteAllocation_succeeds_when_allocation_is_allocated()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        var createBatchResult = await Mediator.Send(new CreateBatchCommand(productId, 10, 10m));
        createBatchResult.IsSuccess.Should().BeTrue();

        var allocateResult = await Mediator.Send(CreateAllocateCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new AllocateOrderLineModel { Id = orderItemId, ProductId = productId, Quantity = 4 }])));
        allocateResult.IsSuccess.Should().BeTrue();

        var allocationId = await GetAllocationIdForOrderAsync(orderId);

        var requestResult = await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId });
        requestResult.IsSuccess.Should().BeTrue();

        // Act
        var result = await Mediator.Send(new CompleteAllocationCommand { AllocationId = allocationId });

        // Assert
        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var allocation = await db.Set<Allocation>().SingleAsync(a => a.Id == allocationId);
        allocation.Status.Should().Be(AllocationStatus.Completed);

        var batch = await db.Set<Batch>().SingleAsync(b => b.ProductId == productId);
        batch.ReservedQuantity.Should().Be(0);
        batch.Quantity.Should().Be(6);
    }

    [Test]
    public async Task CompleteAllocation_fails_when_allocation_is_pending()
    {
        var orderId = Guid.NewGuid().ToString();
        var allocateResult = await Mediator.Send(CreateAllocateCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new AllocateOrderLineModel { Id = $"oi-{Guid.NewGuid():N}", ProductId = Guid.NewGuid().ToString(), Quantity = 1 }])));
        allocateResult.IsSuccess.Should().BeTrue();

        var allocationId = await GetAllocationIdForOrderAsync(orderId);

        var result = await Mediator.Send(new CompleteAllocationCommand { AllocationId = allocationId });

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
        List<AllocateOrderLineModel> items) =>
        new()
        {
            Id = orderId,
            OrderNumber = "ORD-1",
            CustomerId = "cust-1",
            Items = items
        };
}
