using FluentAssertions;
using Invoria.Inventory.Application.Allocations.Commands.CreateAllocate;
using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Application.Returns.Commands.ApproveReturn;
using Invoria.Inventory.Application.Returns.Commands.CreateImmediateReturn;
using Invoria.Inventory.Application.Returns.Commands.ProcessImmediateReturn;
using Invoria.Inventory.Application.Tests.Batches;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ContractReturnStatus = Invoria.Inventory.Contracts.Returns.Enums.ReturnStatus;

namespace Invoria.Inventory.Application.Tests.Returns;

[TestFixture]
public class ProcessImmediateReturnCommandHandlerTests : BatchTestFixture
{
    [Test]
    public async Task Should_complete_return_and_restore_batch_stock()
    {
        var productId = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";
        const int returnQuantity = 2;

        (await Mediator.Send(new CreateBatchCommand(productId, 10, 10m))).IsSuccess.Should().BeTrue();

        (await Mediator.Send(CreateAllocateCommand.FromEvent(NewAllocateEvent(
            orderId,
            [new AllocateOrderLineModel { Id = orderItemId, ProductId = productId, Quantity = 4 }]))))
            .IsSuccess.Should().BeTrue();

        var allocationId = await GetAllocationIdForOrderAsync(orderId);
        (await Mediator.Send(new RequestAllocationCommand { AllocationId = allocationId })).IsSuccess.Should().BeTrue();

        (await Mediator.Send(new CreateImmediateReturnCommand
        {
            OrderId = orderId,
            AllocationId = allocationId,
            Lines =
            [
                new CreateImmediateReturnLineItem
                {
                    OrderItemId = orderItemId,
                    ProductId = productId,
                    Quantity = returnQuantity
                }
            ]
        })).IsSuccess.Should().BeTrue();

        string returnId;
        int batchQuantityAfterAllocation;
        await using (var scope = ServiceProvider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            returnId = (await db.Set<ImmediateReturn>().SingleAsync(r => r.OrderId == orderId)).Id!;
            batchQuantityAfterAllocation = await db.Set<Batch>()
                .Where(b => b.ProductId == productId)
                .SumAsync(b => b.Quantity);
        }

        (await Mediator.Send(new ApproveReturnCommand(returnId))).IsSuccess.Should().BeTrue();

        var result = await Mediator.Send(new ProcessImmediateReturnCommand
        {
            ReturnId = returnId
        });

        result.IsSuccess.Should().BeTrue();

        await using var verifyScope = ServiceProvider.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var processedReturn = await verifyDb.Set<ImmediateReturn>().SingleAsync(r => r.Id == returnId);
        processedReturn.Status.Should().Be(ContractReturnStatus.Completed);

        var batchQuantityAfterReturn = await verifyDb.Set<Batch>()
            .Where(b => b.ProductId == productId)
            .SumAsync(b => b.Quantity);

        batchQuantityAfterReturn.Should().Be(batchQuantityAfterAllocation + returnQuantity);
    }

    private async Task<string> GetAllocationIdForOrderAsync(string orderId)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        return (await db.Set<Invoria.Inventory.Domain.Allocations.Allocation>()
            .SingleAsync(a => a.OrderId == orderId)).Id!;
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
