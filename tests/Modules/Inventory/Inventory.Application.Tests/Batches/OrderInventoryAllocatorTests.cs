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
    public async Task Pre_flight_fails_when_insufficient_stock_and_does_not_reserve()
    {
        var productId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-1-{Guid.NewGuid():N}";
        await Mediator.Send(new CreateBatchCommand(productId, 2, 10m));

        var evt = NewAllocateEvent(
            items: new List<OrderItemModel>
            {
                new() { Id = orderItemId, ProductId = productId, Quantity = 5 }
            });

        var result = await Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().BeOfType<OrderAllocationPreFlightException>();
        var preFlight = (OrderAllocationPreFlightException)result.Exception!;
        preFlight.Errors.Should().ContainSingle();
        preFlight.Errors.Single().ProductId.Should().Be(productId);
        preFlight.Errors.Single().RequestedQuantity.Should().Be(5);
        preFlight.Errors.Single().AvailableQuantity.Should().Be(2);

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var batch = await db.Set<Batch>().SingleAsync(b => b.ProductId == productId);
        batch.ReservedQuantity.Should().Be(0);
        var allocationCount = await db.Set<BatchAllocation>().CountAsync(a => a.OrderItemId == orderItemId);
        allocationCount.Should().Be(0);
    }

    [Test]
    public async Task Allocates_fifo_across_two_batches_for_one_line()
    {
        var productId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-split-{Guid.NewGuid():N}";
        var first = await Mediator.Send(new CreateBatchCommand(productId, 3, 10m));
        var second = await Mediator.Send(new CreateBatchCommand(productId, 4, 11m));

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        var firstId = first.Value!.Id;
        var secondId = second.Value!.Id;
        string.CompareOrdinal(firstId, secondId).Should().BeLessThan(0,
            because: "FIFO uses batch id order; first created batch should sort before second");

        var evt = NewAllocateEvent(
            items: new List<OrderItemModel>
            {
                new() { Id = orderItemId, ProductId = productId, Quantity = 5 }
            });

        var result = await Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocations = await db.Set<BatchAllocation>()
            .Where(a => a.OrderItemId == orderItemId)
            .OrderBy(a => a.BatchId)
            .ToListAsync();

        allocations.Should().HaveCount(2);
        allocations[0].BatchId.Should().Be(firstId);
        allocations[0].QuantityAllocated.Should().Be(3);
        allocations[1].BatchId.Should().Be(secondId);
        allocations[1].QuantityAllocated.Should().Be(2);

        var b1 = await db.Set<Batch>().SingleAsync(b => b.Id == firstId);
        var b2 = await db.Set<Batch>().SingleAsync(b => b.Id == secondId);
        b1.Quantity.Should().Be(0);
        b2.Quantity.Should().Be(2);
        b1.ReservedQuantity.Should().Be(3);
        b2.ReservedQuantity.Should().Be(2);
    }

    [Test]
    public async Task Allocates_from_single_batch()
    {
        var productId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-one-{Guid.NewGuid():N}";
        var created = await Mediator.Send(new CreateBatchCommand(productId, 10, 10m));
        created.IsSuccess.Should().BeTrue();

        var evt = NewAllocateEvent(
            items: new List<OrderItemModel>
            {
                new() { Id = orderItemId, ProductId = productId, Quantity = 4 }
            });

        var result = await Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocation = await db.Set<BatchAllocation>().SingleAsync(a => a.OrderItemId == orderItemId);
        allocation.QuantityAllocated.Should().Be(4);
        var batch = await db.Set<Batch>().SingleAsync(b => b.Id == created.Value!.Id);
        batch.Quantity.Should().Be(6);
        batch.ReservedQuantity.Should().Be(4);
    }

    [Test]
    public async Task Validation_fails_when_order_id_missing()
    {
        var evt = NewAllocateEvent(
            id: " ",
            items: new List<OrderItemModel>
            {
                new() { Id = "i-1", ProductId = Guid.NewGuid().ToString(), Quantity = 1 }
            });

        var result = await Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<OrderAllocationPreFlightException>();
        result.Exception!.Message.Should().Contain("Insufficient stock");
    }

    [Test]
    public async Task Validation_fails_when_no_items()
    {
        var evt = NewAllocateEvent(items: []);

        var result = await Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        result.IsSuccess.Should().BeTrue();
        result.Exception.Should().BeNull();
    }

    [Test]
    public async Task Pre_flight_returns_all_insufficient_products_in_one_exception()
    {
        var p1 = Guid.NewGuid().ToString();
        var p2 = Guid.NewGuid().ToString();

        await Mediator.Send(new CreateBatchCommand(p1, 1, 10m));
        await Mediator.Send(new CreateBatchCommand(p2, 2, 11m));

        var evt = NewAllocateEvent(
            items: new List<OrderItemModel>
            {
                new() { Id = $"oi-{Guid.NewGuid():N}", ProductId = p1, Quantity = 5 },
                new() { Id = $"oi-{Guid.NewGuid():N}", ProductId = p2, Quantity = 6 }
            });

        var result = await Mediator.Send(AllocateOrderCommand.FromEvent(evt));

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().BeOfType<OrderAllocationPreFlightException>();
        var preFlight = (OrderAllocationPreFlightException)result.Exception!;
        preFlight.Errors.Should().HaveCount(2);
        preFlight.Errors.Should().Contain(e =>
            e.ProductId == p1 &&
            e.RequestedQuantity == 5 &&
            e.AvailableQuantity == 1);
        preFlight.Errors.Should().Contain(e =>
            e.ProductId == p2 &&
            e.RequestedQuantity == 6 &&
            e.AvailableQuantity == 2);
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
