using System.Reflection;
using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Application.OrderAllocationConsumptions.Commands.CreateOrderAllocationConsumption;
using Invoria.Ordering.Application.Tests.Orders;
using Invoria.Ordering.Domain.OrderAllocationConsumptions;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Tests.OrderAllocationConsumptions.Commands;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

[TestFixture]
public class CreateOrderAllocationConsumptionCommandHandlerTests : OrderTestFixture
{
    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearDataAsync();
    }

    private async Task ClearDataAsync()
    {
        var db = Scope.Resolve<OrderingDbContext>();
        var batches = await db.Set<OrderAllocationConsumptionBatch>().ToListAsync();
        db.RemoveRange(batches);
        var lines = await db.Set<OrderAllocationConsumptionLine>().ToListAsync();
        db.RemoveRange(lines);
        var consumptions = await db.Set<OrderAllocationConsumption>().ToListAsync();
        db.RemoveRange(consumptions);
        var reports = await db.Set<ReportOrderSalesProfitMetricsEntity>().ToListAsync();
        db.RemoveRange(reports);
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_create_order_allocation_consumption_with_lines_and_batches()
    {
        await SeedOrderAsync();

        var result = await Mediator.Send(BuildCommand());

        result.ShouldBeSuccess();

        var persisted = await Scope.Resolve<OrderingDbContext>()
            .Set<OrderAllocationConsumption>()
            .Include(c => c.Lines)
            .ThenInclude(l => l.BatchAllocations)
            .SingleAsync(c => c.AllocationId == "alloc-1");

        persisted.OrderId.Should().Be("order-1");
        persisted.Lines.Should().HaveCount(1);

        var line = persisted.Lines.Single();
        line.OrderItemId.Should().Be("item-1");
        line.ProductId.Should().Be("product-1");
        line.QuantityRequested.Should().Be(5);

        var batch = line.BatchAllocations.Single();
        batch.BatchId.Should().Be("batch-1");
        batch.Quantity.Should().Be(3);
        batch.UnitPrice.Should().Be(10m);
    }

    [Test]
    public async Task Should_create_multiple_lines_with_their_batch_allocations()
    {
        await SeedOrderAsync();

        var command = BuildCommand();
        command.Lines.Add(new CreateOrderAllocationConsumptionCommand.Line
        {
            OrderItemId = "item-2",
            ProductId = "product-2",
            QuantityRequested = 2,
            BatchAllocations =
            [
                new CreateOrderAllocationConsumptionCommand.Batch
                {
                    BatchId = "batch-2",
                    Quantity = 2,
                    UnitPrice = 7.5m
                }
            ]
        });

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();

        var persisted = await Scope.Resolve<OrderingDbContext>()
            .Set<OrderAllocationConsumption>()
            .Include(c => c.Lines)
            .ThenInclude(l => l.BatchAllocations)
            .SingleAsync(c => c.AllocationId == "alloc-1");

        persisted.Lines.Should().HaveCount(2);
        persisted.Lines.Single(l => l.OrderItemId == "item-2")
            .BatchAllocations.Single().UnitPrice.Should().Be(7.5m);
    }

    [Test]
    public async Task Should_be_idempotent_when_allocation_already_consumed()
    {
        await SeedOrderAsync();

        await Mediator.Send(BuildCommand());

        var result = await Mediator.Send(BuildCommand());

        result.ShouldBeSuccess();

        var count = await Scope.Resolve<OrderingDbContext>()
            .Set<OrderAllocationConsumption>()
            .CountAsync(c => c.AllocationId == "alloc-1");

        count.Should().Be(1);
    }

    private async Task SeedOrderAsync()
    {
        var order = new Order($"CONSUMPTION-{Guid.NewGuid():N}", Guid.NewGuid().ToString());
        AssignStringEntityId(order, "order-1");
        var item1 = new OrderItem("product-1", 5, 20m);
        AssignStringEntityId(item1, "item-1");
        var item2 = new OrderItem("product-2", 2, 15m);
        AssignStringEntityId(item2, "item-2");
        order.UpdateItems([item1, item2]);
        await OrderRepository.Add(order, CancellationToken.None);
    }

    private static void AssignStringEntityId(Entity<string> entity, string id)
    {
        var property = typeof(Entity<string>).GetProperty(
            nameof(Entity<string>.Id),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        property.SetValue(entity, id);
    }

    private static CreateOrderAllocationConsumptionCommand BuildCommand() =>
        new()
        {
            OrderId = "order-1",
            AllocationId = "alloc-1",
            Lines =
            [
                new CreateOrderAllocationConsumptionCommand.Line
                {
                    OrderItemId = "item-1",
                    ProductId = "product-1",
                    QuantityRequested = 5,
                    BatchAllocations =
                    [
                        new CreateOrderAllocationConsumptionCommand.Batch
                        {
                            BatchId = "batch-1",
                            Quantity = 3,
                            UnitPrice = 10m
                        }
                    ]
                }
            ]
        };
}
