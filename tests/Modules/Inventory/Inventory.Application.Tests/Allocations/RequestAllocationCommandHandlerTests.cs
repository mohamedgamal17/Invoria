using FluentAssertions;
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
public class RequestAllocationCommandHandlerTests : Batches.BatchTestFixture
{
    [Test]
    public async Task Allocates_stock_and_marks_allocation_when_sufficient_inventory()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        var batchCreated = await Mediator.Send(new CreateBatchCommand(productId, 10, 10m));
        batchCreated.IsSuccess.Should().BeTrue();

        var allocateResult = await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = orderItemId, ProductId = productId, Quantity = 4 }])));
        allocateResult.IsSuccess.Should().BeTrue();

        var allocationId = await GetAllocationIdForOrderAsync(orderId);

        var result = await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var allocation = await db.Set<Allocation>()
            .Include(a => a.Lines)
            .ThenInclude(l => l.BatchAllocations)
            .SingleAsync(a => a.Id == allocationId);

        allocation.Status.Should().Be(AllocationStatus.Allocated);
        allocation.Lines.Should().ContainSingle();
        allocation.Lines.Single().Status.Should().Be(AllocationLineStatus.Allocated);
        allocation.Lines.Single().BatchAllocations.Should().ContainSingle();
        allocation.Lines.Single().BatchAllocations.Single().QuantityAllocated.Should().Be(4);

        var batch = await db.Set<Batch>().SingleAsync(b => b.Id == batchCreated.Value!.Id);
        batch.Quantity.Should().Be(6);
        batch.ReservedQuantity.Should().Be(4);
    }

    [Test]
    public async Task Consumes_older_batch_first_when_multiple_batches_exist()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        var olderBatch = await Mediator.Send(new CreateBatchCommand(productId, 3, 10m));
        var newerBatch = await Mediator.Send(new CreateBatchCommand(productId, 5, 10m));
        olderBatch.IsSuccess.Should().BeTrue();
        newerBatch.IsSuccess.Should().BeTrue();

        await using (var scope = ServiceProvider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            await SetBatchCreatedAtAsync(db, olderBatch.Value!.Id, DateTimeOffset.UtcNow.AddDays(-2));
            await SetBatchCreatedAtAsync(db, newerBatch.Value!.Id, DateTimeOffset.UtcNow.AddDays(-1));
        }

        await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = orderItemId, ProductId = productId, Quantity = 5 }])));

        var allocationId = await GetAllocationIdForOrderAsync(orderId);
        var result = await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeTrue();

        await using var assertScope = ServiceProvider.CreateAsyncScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var older = await assertDb.Set<Batch>().SingleAsync(b => b.Id == olderBatch.Value!.Id);
        var newer = await assertDb.Set<Batch>().SingleAsync(b => b.Id == newerBatch.Value!.Id);

        older.Quantity.Should().Be(0);
        older.ReservedQuantity.Should().Be(3);
        newer.Quantity.Should().Be(3);
        newer.ReservedQuantity.Should().Be(2);

        var lineAllocations = await assertDb.Set<BatchAllocation>()
            .Where(a => a.OrderItemId == orderItemId)
            .ToListAsync();

        lineAllocations.Should().HaveCount(2);
        lineAllocations.Should().Contain(a =>
            a.BatchId == olderBatch.Value!.Id && a.QuantityAllocated == 3);
        lineAllocations.Should().Contain(a =>
            a.BatchId == newerBatch.Value!.Id && a.QuantityAllocated == 2);
    }

    [Test]
    public async Task Marks_line_failed_with_partial_batch_allocations_when_stock_insufficient()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        var batchCreated = await Mediator.Send(new CreateBatchCommand(productId, 3, 10m));
        batchCreated.IsSuccess.Should().BeTrue();

        await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = orderItemId, ProductId = productId, Quantity = 4 }])));

        var allocationId = await GetAllocationIdForOrderAsync(orderId);
        var result = await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var allocation = await db.Set<Allocation>()
            .Include(a => a.Lines)
            .ThenInclude(l => l.BatchAllocations)
            .SingleAsync(a => a.Id == allocationId);

        allocation.Status.Should().Be(AllocationStatus.Failed);
        var line = allocation.Lines.Single();
        line.Status.Should().Be(AllocationLineStatus.Failed);
        line.BatchAllocations.Should().ContainSingle();
        line.BatchAllocations.Single().QuantityAllocated.Should().Be(3);

        var batch = await db.Set<Batch>().SingleAsync(b => b.Id == batchCreated.Value!.Id);
        batch.Quantity.Should().Be(3);
        batch.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public async Task Allocates_successful_lines_and_marks_short_lines_failed_with_batch_allocations()
    {
        var productA = Guid.NewGuid().ToString();
        var productB = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemA = $"oi-a-{Guid.NewGuid():N}";
        var orderItemB = $"oi-b-{Guid.NewGuid():N}";

        await Mediator.Send(new CreateBatchCommand(productA, 10, 10m));
        await Mediator.Send(new CreateBatchCommand(productB, 1, 10m));

        await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [
                new OrderItemModel { Id = orderItemA, ProductId = productA, Quantity = 2 },
                new OrderItemModel { Id = orderItemB, ProductId = productB, Quantity = 3 }
            ])));

        var allocationId = await GetAllocationIdForOrderAsync(orderId);
        var result = await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var allocation = await db.Set<Allocation>()
            .Include(a => a.Lines)
            .ThenInclude(l => l.BatchAllocations)
            .SingleAsync(a => a.Id == allocationId);

        allocation.Status.Should().Be(AllocationStatus.Failed);

        var lineA = allocation.Lines.Single(l => l.OrderItemId == orderItemA);
        lineA.Status.Should().Be(AllocationLineStatus.Released);
        lineA.BatchAllocations.Should().ContainSingle();

        var lineB = allocation.Lines.Single(l => l.OrderItemId == orderItemB);
        lineB.Status.Should().Be(AllocationLineStatus.Failed);
        lineB.BatchAllocations.Should().ContainSingle();
        lineB.BatchAllocations.Single().QuantityAllocated.Should().Be(1);

        var batches = await db.Set<Batch>().Where(b => b.ProductId == productA || b.ProductId == productB).ToListAsync();
        batches.Single(b => b.ProductId == productA).Quantity.Should().Be(10);
        batches.Single(b => b.ProductId == productA).ReservedQuantity.Should().Be(0);
        batches.Single(b => b.ProductId == productB).Quantity.Should().Be(1);
        batches.Single(b => b.ProductId == productB).ReservedQuantity.Should().Be(0);
    }

    [Test]
    public async Task Second_request_is_no_op_when_lines_already_processed()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        await Mediator.Send(new CreateBatchCommand(productId, 10, 10m));
        await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = orderItemId, ProductId = productId, Quantity = 2 }])));

        var allocationId = await GetAllocationIdForOrderAsync(orderId);

        var first = await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId });
        var second = await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId });

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var batchAllocationCount = await db.Set<BatchAllocation>().CountAsync(a => a.OrderItemId == orderItemId);
        batchAllocationCount.Should().Be(1);
    }

    private async Task<string> GetAllocationIdForOrderAsync(string orderId)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocation = await db.Set<Allocation>().SingleAsync(a => a.OrderId == orderId);
        return allocation.Id!;
    }

    private static async Task SetBatchCreatedAtAsync(
        InventoryDbContext db,
        string batchId,
        DateTimeOffset createdAt)
    {
        var entry = await db.Set<Batch>().SingleAsync(b => b.Id == batchId);
        db.Entry(entry).Property(nameof(Batch.CreatedAt)).CurrentValue = createdAt;
        await db.SaveChangesAsync();
    }

    private static AllocateOrderIntegrationEvent NewAllocateEvent(
        string orderId,
        List<OrderItemModel> items) =>
        new()
        {
            Id = orderId,
            OrderNumber = "ORD-TEST",
            CustomerId = Guid.NewGuid().ToString(),
            Items = items
        };
}
