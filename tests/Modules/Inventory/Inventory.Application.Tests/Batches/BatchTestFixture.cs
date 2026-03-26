using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Batches;

public class BatchTestFixture : InventoryTestFixture
{
    protected IMediator Mediator { get; }

    public BatchTestFixture()
    {
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }
}
