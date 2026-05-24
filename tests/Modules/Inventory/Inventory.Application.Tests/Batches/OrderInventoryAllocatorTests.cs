using FluentAssertions;
using Invoria.Inventory.Application.Batches.Commands.AllocateOrder;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class OrderInventoryAllocatorTests : BatchTestFixture
{
    [Test]
    public async Task Creates_allocation_with_lines()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-one-{Guid.NewGuid():N}";
        var created = await Mediator.Send(new CreateBatchCommand(productId, 10, 10m));
        created.IsSuccess.Should().BeTrue();

        var evt = NewAllocateEvent(
            id: orderId,
            items: new List<OrderItemModel>
            {
                new() { Id = orderItemId, ProductId = productId, Quantity = 4 }
            });

        var result = await Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var allocation = await db.Set<Allocation>()
            .Include(a => a.Lines)
            .SingleAsync(a => a.OrderId == orderId);

        allocation.Status.Should().Be(AllocationStatus.Pending);
        allocation.Lines.Should().ContainSingle();
        allocation.Lines.Single().OrderItemId.Should().Be(orderItemId);
        allocation.Lines.Single().ProductId.Should().Be(productId);
        allocation.Lines.Single().QuantityRequested.Should().Be(4);
        allocation.Lines.Single().Status.Should().Be(AllocationLineStatus.Pending);

        var batchAllocationCount = await db.Set<BatchAllocation>().CountAsync(a => a.OrderItemId == orderItemId);
        batchAllocationCount.Should().Be(0);

        var batch = await db.Set<Batch>().SingleAsync(b => b.Id == created.Value!.Id);
        batch.Quantity.Should().Be(10);
        batch.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public async Task Fails_with_empty_items()
    {
        var orderId = Guid.NewGuid().ToString();
        var evt = NewAllocateEvent(id: orderId, items: []);

        var act = () => Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        await act.Should().ThrowAsync<ArgumentException>();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocationCount = await db.Set<Allocation>().CountAsync(a => a.OrderId == orderId);
        allocationCount.Should().Be(0);
    }

    [Test]
    public async Task Fails_when_order_id_missing()
    {
        var evt = NewAllocateEvent(
            id: " ",
            items: new List<OrderItemModel>
            {
                new() { Id = "i-1", ProductId = Guid.NewGuid().ToString(), Quantity = 1 }
            });

        var act = () => Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        await act.Should().ThrowAsync<ArgumentException>();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocationCount = await db.Set<Allocation>().CountAsync(a => a.OrderId == " ");
        allocationCount.Should().Be(0);
    }

    [Test]
    public async Task Fails_when_line_quantity_invalid()
    {
        var evt = NewAllocateEvent(
            items: new List<OrderItemModel>
            {
                new() { Id = "i-1", ProductId = Guid.NewGuid().ToString(), Quantity = 0 }
            });

        var act = () => Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task Adds_new_allocation_on_each_call()
    {
        var orderId = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid().ToString();

        var evt = NewAllocateEvent(
            id: orderId,
            items: new List<OrderItemModel>
            {
                new() { Id = $"oi-{Guid.NewGuid():N}", ProductId = productId, Quantity = 1 }
            });

        var first = await Mediator.Send(AllocateOrderCommand.FromEvent(evt));
        var second = await Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocations = await db.Set<Allocation>().Where(a => a.OrderId == orderId).ToListAsync();
        allocations.Should().HaveCount(2);
    }

    private static AllocateOrderIntegrationEvent NewAllocateEvent(
        List<OrderItemModel> items,
        string? id = null,
        string? orderNumber = null,
        string? customerId = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid().ToString(),
            OrderNumber = orderNumber ?? "ORD-TEST",
            CustomerId = customerId ?? Guid.NewGuid().ToString(),
            Items = items
        };
}
