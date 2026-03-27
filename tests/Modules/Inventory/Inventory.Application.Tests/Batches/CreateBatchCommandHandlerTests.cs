using FluentAssertions;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Application.Tests.Assertions;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class CreateBatchCommandHandlerTests : BatchTestFixture
{
    [Test]
    public async Task Should_create_batch()
    {
        var productId = Guid.NewGuid().ToString();
        const int quantity = 10;
        const decimal purchasePrice = 25.5m;

        var command = new CreateBatchCommand(productId, quantity, purchasePrice);

        var result = await Mediator.Send(command);

        Assert.That(result.IsSuccess, Is.True);
        result.Value!.AssertBatchDto(command);

        var dbContext = ServiceProvider.GetRequiredService<InventoryDbContext>();
        var batch = await dbContext.Set<Batch>()
            .SingleOrDefaultAsync(x => x.Id == result.Value!.Id);

        Assert.That(batch, Is.Not.Null);
        batch!.AssertCreateBatchCommand(command);
        batch.State.Should().Be(BatchState.Active);
    }
}
