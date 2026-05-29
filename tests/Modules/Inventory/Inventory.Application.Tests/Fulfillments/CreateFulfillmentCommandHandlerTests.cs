using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Application.Batches.Commands.AllocateOrder;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Application.Fulfillments.Commands.CreateFulfillment;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Fulfillments;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Fulfillments;

[TestFixture]
public class CreateFulfillmentCommandHandlerTests : Batches.BatchTestFixture
{
    [Test]
    public async Task CreateFulfillment_succeeds_when_allocation_is_allocated()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        (await Mediator.Send(new CreateBatchCommand(productId, 10, 10m))).IsSuccess.Should().BeTrue();

        (await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = orderItemId, ProductId = productId, Quantity = 4 }]))))
            .IsSuccess.Should().BeTrue();

        var allocationId = await GetAllocationIdForOrderAsync(orderId);

        (await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId }))
            .IsSuccess.Should().BeTrue();

        var result = await Mediator.Send(new CreateFulfillmentCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var fulfillment = await db.Set<Fulfillment>()
            .SingleAsync(f => f.AllocationId == allocationId);

        var items = await db.Set<FulfillmentItem>()
            .Where(i => i.FulfillmentId == fulfillment.Id)
            .ToListAsync();

        fulfillment.OrderId.Should().Be(orderId);
        fulfillment.Status.Should().Be(FulfillmentStatus.Pending);
        items.Should().HaveCount(1);
        items[0].ProductId.Should().Be(productId);
        items[0].AllocatedQuantity.Should().Be(4);
    }

    [Test]
    public async Task CreateFulfillment_fails_when_allocation_does_not_exist()
    {
        var result = await Mediator.Send(new CreateFulfillmentCommand
        {
            AllocationId = Guid.NewGuid().ToString()
        });

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().BeOfType<NotFoundException>();
    }

    [Test]
    public async Task CreateFulfillment_fails_when_allocation_is_not_allocated()
    {
        var orderId = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid().ToString();

        (await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = "oi-1", ProductId = productId, Quantity = 4 }]))))
            .IsSuccess.Should().BeTrue();

        var allocationId = await GetAllocationIdForOrderAsync(orderId);

        var result = await Mediator.Send(new CreateFulfillmentCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeFalse();
    }

    private async Task<string> GetAllocationIdForOrderAsync(string orderId)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocation = await db.Set<Allocation>().SingleAsync(a => a.OrderId == orderId);
        return allocation.Id!;
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
