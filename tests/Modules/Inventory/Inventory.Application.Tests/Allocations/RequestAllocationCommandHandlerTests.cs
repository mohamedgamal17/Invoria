using FluentAssertions;
using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Application.Allocations.Commands.CreateAllocate;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class RequestAllocationCommandHandlerTests : Batches.BatchTestFixture
{
    [Test]
    public async Task RequestAllocation_succeeds_when_inventory_is_sufficient()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        (await Mediator.Send(new CreateBatchCommand(productId, 10, 10m))).IsSuccess.Should().BeTrue();

        (await Mediator.Send(CreateAllocateCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new AllocateOrderLineModel { Id = orderItemId, ProductId = productId, Quantity = 4 }]))))
            .IsSuccess.Should().BeTrue();

        var allocationId = await GetAllocationIdForOrderAsync(orderId);

        var result = await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var allocation = await db.Set<Allocation>()
            .Include(a => a.Lines)
            .SingleAsync(a => a.Id == allocationId);

        allocation.Status.Should().Be(AllocationStatus.Allocated);
        allocation.Lines.Should().OnlyContain(l => l.Status == AllocationLineStatus.Allocated);
    }

    [Test]
    public async Task RequestAllocation_fails_when_inventory_is_insufficient()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";

        (await Mediator.Send(new CreateBatchCommand(productId, 3, 10m))).IsSuccess.Should().BeTrue();

        (await Mediator.Send(CreateAllocateCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new AllocateOrderLineModel { Id = orderItemId, ProductId = productId, Quantity = 4 }]))))
            .IsSuccess.Should().BeTrue();

        var allocationId = await GetAllocationIdForOrderAsync(orderId);

        var result = await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId });

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var allocation = await db.Set<Allocation>()
            .Include(a => a.Lines)
            .SingleAsync(a => a.Id == allocationId);

        allocation.Status.Should().Be(AllocationStatus.Failed);
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
        List<AllocateOrderLineModel> items) =>
        new()
        {
            Id = orderId,
            OrderNumber = "ORD-TEST",
            CustomerId = Guid.NewGuid().ToString(),
            Items = items
        };
}
