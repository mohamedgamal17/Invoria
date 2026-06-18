using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Domain.Returns.Services;
using ContractReturnStatus = Invoria.Inventory.Contracts.Returns.Enums.ReturnStatus;

namespace Invoria.Inventory.Application.Tests.Domain.Returns;

[TestFixture]
public class ReturnDomainServiceTests
{
    private readonly ReturnDomainService _sut = new();

    [Test]
    public void ProcessImmediateReturn_restores_stock_in_descending_batch_id_order()
    {
        const string orderItemId = "oi-1";
        const string productId = "p-1";
        var olderAt = DateTimeOffset.UtcNow.AddHours(-2);
        var newerAt = DateTimeOffset.UtcNow.AddHours(-1);

        var allocation = CreateAllocatedAllocation(
            orderItemId,
            productId,
            ("batch-1", 3, olderAt),
            ("batch-2", 4, newerAt));

        var olderBatch = new Batch(productId, 0, 10m);
        SetEntityId(olderBatch, "batch-1");
        var newerBatch = new Batch(productId, 0, 10m);
        SetEntityId(newerBatch, "batch-2");

        var immediateReturn = CreateApprovedImmediateReturn(
            allocation.Id!,
            orderItemId,
            productId,
            returnQuantity: 5);

        _sut.ProcessImmediateReturn(
            immediateReturn,
            allocation,
            [olderBatch, newerBatch]);

        newerBatch.Quantity.Should().Be(4);
        olderBatch.Quantity.Should().Be(1);
        immediateReturn.Status.Should().Be(ContractReturnStatus.Completed);
    }

    [Test]
    public void ProcessImmediateReturn_stops_when_remaining_is_satisfied()
    {
        const string orderItemId = "oi-1";
        const string productId = "p-1";
        var olderAt = DateTimeOffset.UtcNow.AddHours(-2);
        var newerAt = DateTimeOffset.UtcNow.AddHours(-1);

        var allocation = CreateAllocatedAllocation(
            orderItemId,
            productId,
            ("batch-1", 3, olderAt),
            ("batch-2", 4, newerAt));

        var olderBatch = new Batch(productId, 0, 10m);
        SetEntityId(olderBatch, "batch-1");
        var newerBatch = new Batch(productId, 0, 10m);
        SetEntityId(newerBatch, "batch-2");

        var immediateReturn = CreateApprovedImmediateReturn(
            allocation.Id!,
            orderItemId,
            productId,
            returnQuantity: 2);

        _sut.ProcessImmediateReturn(
            immediateReturn,
            allocation,
            [olderBatch, newerBatch]);

        newerBatch.Quantity.Should().Be(2);
        olderBatch.Quantity.Should().Be(0);
        immediateReturn.Status.Should().Be(ContractReturnStatus.Completed);
    }

    [Test]
    public void ProcessImmediateReturn_marks_immediate_return_completed()
    {
        const string orderItemId = "oi-1";
        const string productId = "p-1";
        var allocatedAt = DateTimeOffset.UtcNow;

        var allocation = CreateAllocatedAllocation(
            orderItemId,
            productId,
            ("batch-1", 3, allocatedAt));

        var batch = new Batch(productId, 0, 10m);
        SetEntityId(batch, "batch-1");

        var immediateReturn = CreateApprovedImmediateReturn(
            allocation.Id!,
            orderItemId,
            productId,
            returnQuantity: 3);

        _sut.ProcessImmediateReturn(immediateReturn, allocation, [batch]);

        immediateReturn.Status.Should().Be(ContractReturnStatus.Completed);
        batch.Quantity.Should().Be(3);
    }

    private static Allocation CreateAllocatedAllocation(
        string orderItemId,
        string productId,
        params (string BatchId, int Quantity, DateTimeOffset AllocatedAt)[] batchAllocations)
    {
        var totalQuantity = batchAllocations.Sum(a => a.Quantity);
        var allocation = Allocation.CreateForOrder(
            "order-1",
            [(orderItemId, productId, totalQuantity)]);

        var line = allocation.Lines.Single();
        foreach (var (batchId, quantity, allocatedAt) in batchAllocations)
        {
            line.RecordBatchAllocation(new BatchAllocation(batchId, orderItemId, quantity, allocatedAt));
        }

        line.MarkAsAllocated();
        allocation.MarkAsAllocated();
        return allocation;
    }

    private static ImmediateReturn CreateApprovedImmediateReturn(
        string allocationId,
        string orderItemId,
        string productId,
        int returnQuantity)
    {
        var immediateReturn = ImmediateReturn.Create(
            allocationId,
            "order-1",
            [ReturnLine.Create(orderItemId, productId, returnQuantity)]);

        immediateReturn.Approve();
        return immediateReturn;
    }

    private static void SetEntityId(Batch batch, string id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, id);
}
