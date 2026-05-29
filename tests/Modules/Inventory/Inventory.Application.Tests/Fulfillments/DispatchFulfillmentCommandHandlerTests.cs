using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Application.Batches.Commands.AllocateOrder;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Application.Fulfillments.Commands.CreateFulfillment;
using Invoria.Inventory.Application.Fulfillments.Commands.DispatchFulfillment;
using Invoria.Inventory.Application.Fulfillments.Commands.RequestDispatchFulfillment;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Domain.Fulfillments;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Fulfillments;

[TestFixture]
public class DispatchFulfillmentCommandHandlerTests : Batches.BatchTestFixture
{
    [Test]
    public async Task DispatchFulfillment_succeeds_when_fulfillment_is_in_progress()
    {
        var productId = Guid.NewGuid().ToString();
        var fulfillmentId = await CreateInProgressFulfillmentAsync(productId);

        var result = await Mediator.Send(new DispatchFulfillmentCommand { FulfillmentId = fulfillmentId });

        result.IsSuccess.Should().BeTrue(result.Exception?.Message ?? "unknown failure");

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var fulfillment = await db.Set<Fulfillment>().SingleAsync(f => f.Id == fulfillmentId);
        fulfillment.Status.Should().Be(FulfillmentStatus.Completed);

        var allocation = await db.Set<Allocation>().SingleAsync(a => a.Id == fulfillment.AllocationId);
        allocation.Status.Should().Be(AllocationStatus.Allocated);

        var batch = await db.Set<Batch>().SingleAsync(b => b.ProductId == productId);
        batch.ReservedQuantity.Should().Be(0);
        batch.Quantity.Should().Be(6);
    }

    [Test]
    public async Task DispatchFulfillment_fails_when_fulfillment_does_not_exist()
    {
        var result = await Mediator.Send(new DispatchFulfillmentCommand
        {
            FulfillmentId = Guid.NewGuid().ToString()
        });

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().BeOfType<NotFoundException>();
    }

    [Test]
    public async Task DispatchFulfillment_fails_when_fulfillment_is_not_in_progress()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        (await Mediator.Send(new CreateBatchCommand(productId, 10, 10m))).IsSuccess.Should().BeTrue();

        (await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = orderItemId, ProductId = productId, Quantity = 4 }]))))
            .IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocationId = (await db.Set<Allocation>().SingleAsync(a => a.OrderId == orderId)).Id!;

        (await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId }))
            .IsSuccess.Should().BeTrue();

        (await Mediator.Send(new CreateFulfillmentCommand { AllocationId = allocationId }))
            .IsSuccess.Should().BeTrue();

        var fulfillmentId = (await db.Set<Fulfillment>().SingleAsync(f => f.AllocationId == allocationId)).Id!;

        var result = await Mediator.Send(new DispatchFulfillmentCommand { FulfillmentId = fulfillmentId });

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().BeOfType<BusinessLogicException>()
            .Which.Message.Should().Contain("InProgress");
    }

    private async Task<string> CreateInProgressFulfillmentAsync(string productId)
    {
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        (await Mediator.Send(new CreateBatchCommand(productId, 10, 10m))).IsSuccess.Should().BeTrue();

        (await Mediator.Send(AllocateOrderCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new OrderItemModel { Id = orderItemId, ProductId = productId, Quantity = 4 }]))))
            .IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocationId = (await db.Set<Allocation>().SingleAsync(a => a.OrderId == orderId)).Id!;

        (await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId }))
            .IsSuccess.Should().BeTrue();

        (await Mediator.Send(new CreateFulfillmentCommand { AllocationId = allocationId }))
            .IsSuccess.Should().BeTrue();

        var fulfillmentId = (await db.Set<Fulfillment>().SingleAsync(f => f.AllocationId == allocationId)).Id!;

        (await Mediator.Send(new RequestDispatchFulfillmentCommand { FulfillmentId = fulfillmentId }))
            .IsSuccess.Should().BeTrue();

        return fulfillmentId;
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
