using FluentAssertions;
using Invoria.Inventory.Application.Batches.Commands.AllocateOrder;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Application.Batches.Commands.DispatchOrder;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class OrderDispatchInventoryCommandHandlerTests : BatchTestFixture
{
    [Test]
    public async Task Dispatch_releases_reserved_and_retains_allocations()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-d1-{Guid.NewGuid():N}";

        await Mediator.Send(new CreateBatchCommand(productId, 10, 10m));

        var allocateEvt = new AllocateOrderIntegrationEvent
        {
            Id = orderId,
            OrderNumber = "ORD-D1",
            CustomerId = Guid.NewGuid().ToString(),
            Items = new List<OrderItemModel>
            {
                new() { Id = orderItemId, ProductId = productId, Quantity = 4 }
            }
        };

        var allocateResult = await Mediator.Send(AllocateOrderCommand.FromEvent(allocateEvt));
        allocateResult.IsSuccess.Should().BeTrue();

        var dispatchCmd = new OrderDispatchedIntegrationEvent
        {
            Id = orderId,
            OrderNumber = "ORD-D1",
            CustomerId = allocateEvt.CustomerId,
            DispatchedAt = DateTimeOffset.UtcNow,
            Items = new List<OrderItemModel>
            {
                new() { Id = orderItemId, ProductId = productId, Quantity = 4 }
            }
        };

        var dispatchResult = await Mediator.Send(DispatchOrderCommand.FromEvent(dispatchCmd));
        dispatchResult.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var batch = await db.Set<Batch>().SingleAsync(b => b.ProductId == productId);
        batch.ReservedQuantity.Should().Be(0);
        batch.Quantity.Should().Be(6);

        var allocationCount = await db.Set<BatchAllocation>().CountAsync(a => a.OrderItemId == orderItemId);
        allocationCount.Should().Be(1);

        var processed = await db.Set<OrderDispatchProcessed>().SingleOrDefaultAsync(p => p.Id == orderId);
        processed.Should().NotBeNull();
        processed!.ProcessedAt.Should().NotBe(default(DateTimeOffset));
    }

    [Test]
    public async Task Dispatch_is_idempotent_when_called_again()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-d2-{Guid.NewGuid():N}";

        await Mediator.Send(new CreateBatchCommand(productId, 5, 10m));

        var allocateEvt = new AllocateOrderIntegrationEvent
        {
            Id = orderId,
            OrderNumber = "ORD-D2",
            CustomerId = Guid.NewGuid().ToString(),
            Items = new List<OrderItemModel>
            {
                new() { Id = orderItemId, ProductId = productId, Quantity = 3 }
            }
        };

        await Mediator.Send(AllocateOrderCommand.FromEvent(allocateEvt));

        var dispatchCmd = new OrderDispatchedIntegrationEvent
        {
            Id = orderId,
            OrderNumber = "ORD-D2",
            CustomerId = allocateEvt.CustomerId,
            DispatchedAt = DateTimeOffset.UtcNow,
            Items = new List<OrderItemModel>
            {
                new() { Id = orderItemId, ProductId = productId, Quantity = 3 }
            }
        };

        var first = await Mediator.Send(DispatchOrderCommand.FromEvent(dispatchCmd));
        first.IsSuccess.Should().BeTrue();

        var second = await Mediator.Send(DispatchOrderCommand.FromEvent(dispatchCmd));
        second.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Dispatch_releases_across_split_allocations()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-split-d-{Guid.NewGuid():N}";

        await Mediator.Send(new CreateBatchCommand(productId, 3, 10m));
        await Mediator.Send(new CreateBatchCommand(productId, 4, 11m));

        var allocateEvt = new AllocateOrderIntegrationEvent
        {
            Id = orderId,
            OrderNumber = "ORD-SD",
            CustomerId = Guid.NewGuid().ToString(),
            Items = new List<OrderItemModel>
            {
                new() { Id = orderItemId, ProductId = productId, Quantity = 5 }
            }
        };

        await Mediator.Send(AllocateOrderCommand.FromEvent(allocateEvt));

        var dispatchCmd = new OrderDispatchedIntegrationEvent
        {
            Id = orderId,
            OrderNumber = "ORD-SD",
            CustomerId = allocateEvt.CustomerId,
            DispatchedAt = DateTimeOffset.UtcNow,
            Items = new List<OrderItemModel>
            {
                new() { Id = orderItemId, ProductId = productId, Quantity = 5 }
            }
        };

        var dispatchResult = await Mediator.Send(DispatchOrderCommand.FromEvent(dispatchCmd));
        dispatchResult.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var batches = await db.Set<Batch>().Where(b => b.ProductId == productId).ToListAsync();
        batches.Should().AllSatisfy(b => b.ReservedQuantity.Should().Be(0));
        (await db.Set<BatchAllocation>().CountAsync(a => a.OrderItemId == orderItemId)).Should().Be(2);
    }
}
