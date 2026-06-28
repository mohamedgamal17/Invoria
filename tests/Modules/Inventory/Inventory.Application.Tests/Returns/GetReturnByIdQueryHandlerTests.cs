using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Inventory.Application.Returns.Queries.GetReturnById;
using Invoria.Inventory.Application.Tests.Assertions;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Returns;
using Microsoft.Extensions.DependencyInjection;

using Invoria.Inventory.Application.Tests.Batches;

namespace Invoria.Inventory.Application.Tests.Returns;

[TestFixture]
public class GetReturnByIdQueryHandlerTests : BatchTestFixture
{
    private readonly IInventoryRepository<Return> _returnRepository;

    public GetReturnByIdQueryHandlerTests()
    {
        _returnRepository = ServiceProvider.GetRequiredService<IInventoryRepository<Return>>();
    }

    [Test]
    public async Task Should_return_return_when_found()
    {
        var returnEntity = ImmediateReturn.Create(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
        [
            ReturnLine.Create($"oi-{Guid.NewGuid():N}", Guid.NewGuid().ToString(), 3)
        ]);

        await _returnRepository.Add(returnEntity);

        var query = new GetReturnByIdQuery { Id = returnEntity.Id };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value!.AssertReturnDto(returnEntity);
    }

    [Test]
    public async Task Should_return_failure_when_return_not_found()
    {
        var query = new GetReturnByIdQuery { Id = Guid.NewGuid().ToString() };

        var result = await Mediator.Send(query);

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NotFoundException>();
    }
}
