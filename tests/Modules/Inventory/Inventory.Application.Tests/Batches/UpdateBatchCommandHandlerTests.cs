using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Inventory.Application.Batches.Commands.UpdateBatch;
using Invoria.Inventory.Application.Tests.Assertions;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class UpdateBatchCommandHandlerTests : BatchTestFixture
{
    private readonly IInventoryRepository<Batch> _batchRepository;

    public UpdateBatchCommandHandlerTests()
    {
        _batchRepository = ServiceProvider.GetRequiredService<IInventoryRepository<Batch>>();
    }

    [Test]
    public async Task Should_update_batch()
    {
        var batch = await _batchRepository.Add(new Batch(Guid.NewGuid().ToString(), 10, 25.5m));
        var command = new UpdateBatchCommand(batch.Id, 7, 20m);

        var result = await Mediator.Send(command);

        Assert.That(result.IsSuccess, Is.True);
        result.Value!.AssertBatchDto(command, batch.ProductId, batch.ReservedQuantity);

        var updated = await _batchRepository.SingleOrDefault(x => x.Id == batch.Id);
        updated.Should().NotBeNull();
        updated!.AssertUpdateBatchCommand(command, batch.ProductId, batch.ReservedQuantity);
    }

    [Test]
    public async Task Should_allow_zero_quantity_when_updating()
    {
        var batch = await _batchRepository.Add(new Batch(Guid.NewGuid().ToString(), 10, 25.5m));
        var command = new UpdateBatchCommand(batch.Id, 0, 20m);

        var result = await Mediator.Send(command);

        Assert.That(result.IsSuccess, Is.True);
        result.Value!.Quantity.Should().Be(0);
    }

    [Test]
    public async Task Should_return_failure_when_batch_not_found()
    {
        var command = new UpdateBatchCommand(Guid.NewGuid().ToString(), 0, 20m);

        var result = await Mediator.Send(command);

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NotFoundException>();
    }
}
