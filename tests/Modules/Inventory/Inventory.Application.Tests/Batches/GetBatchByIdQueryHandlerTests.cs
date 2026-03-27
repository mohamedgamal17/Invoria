using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Inventory.Application.Batches.Queries.GetBatchById;
using Invoria.Inventory.Application.Tests.Assertions;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class GetBatchByIdQueryHandlerTests : BatchTestFixture
{
    private readonly IInventoryRepository<Batch> _batchRepository;

    public GetBatchByIdQueryHandlerTests()
    {
        _batchRepository = ServiceProvider.GetRequiredService<IInventoryRepository<Batch>>();
    }

    [Test]
    public async Task Should_return_batch_when_found()
    {
        var batch = await _batchRepository.Add(new Batch(Guid.NewGuid().ToString(), 10, 25.5m));
        var query = new GetBatchByIdQuery { Id = batch.Id };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value!.AssertBatchDto(batch);
    }

    [Test]
    public async Task Should_return_failure_when_batch_not_found()
    {
        var query = new GetBatchByIdQuery { Id = Guid.NewGuid().ToString() };

        var result = await Mediator.Send(query);

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NotFoundException>();
    }
}
