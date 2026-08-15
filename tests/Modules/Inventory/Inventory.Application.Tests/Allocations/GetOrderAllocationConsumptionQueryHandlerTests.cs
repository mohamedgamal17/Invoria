using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Inventory.Application.Allocations.Commands.CreateAllocate;
using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Application.Allocations.Queries.GetOrderAllocationConsumption;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class GetOrderAllocationConsumptionQueryHandlerTests : Batches.BatchTestFixture
{
    [Test]
    public async Task GetOrderAllocationConsumption_returns_lines_with_batch_allocations_and_unit_prices()
    {
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

        var query = new GetOrderAllocationConsumptionQuery { AllocationId = allocationId };

        var result = await Mediator.Send(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.OrderId.Should().Be(orderId);
        result.Value.AllocationId.Should().Be(allocationId);
        result.Value.Lines.Should().HaveCount(1);

        var line = result.Value.Lines[0];
        line.OrderItemId.Should().Be(orderItemId);
        line.ProductId.Should().Be(productId);
        line.QuantityRequested.Should().Be(4);
        line.BatchAllocations.Should().HaveCount(1);

        var batchAllocation = line.BatchAllocations[0];
        batchAllocation.Quantity.Should().Be(4);
        batchAllocation.UnitPrice.Should().Be(10m);

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var batch = await db.Set<Batch>().SingleAsync(b => b.ProductId == productId);
        batchAllocation.BatchId.Should().Be(batch.Id);
    }

    [Test]
    public async Task GetOrderAllocationConsumption_fails_when_allocation_not_found()
    {
        var result = await Mediator.Send(
            new GetOrderAllocationConsumptionQuery { AllocationId = "missing-allocation" });

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().BeOfType<NotFoundException>();
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
