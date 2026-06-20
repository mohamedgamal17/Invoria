using Invoria.Application.Tests.Extensions;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Application.Tests.Assertions;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Orders.Commands;

[TestFixture]
public class CreateOrderCommandHandlerCatalogMappingTests : OrderTestFixture
{
    protected override void RegisterProductService(IServiceCollection services)
    {
        services.AddSingleton<IProductService, SyntheticListProductService>();
    }

    [Test]
    public async Task Should_attach_product_dto_per_line_from_single_catalog_batch()
    {
        var productId1 = Guid.NewGuid().ToString();
        var productId2 = Guid.NewGuid().ToString();
        var command = new CreateOrderCommand(
            Guid.NewGuid().ToString(),
            new List<CreateOrderItemCommand>
            {
                new(productId1, 2, 10m),
                new(productId2, 1, 25m)
            });

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        ProductDto Expected(string id) => new()
        {
            Id = id,
            Name = SyntheticListProductService.NameForId(id),
            Price = 9.99m
        };

        result.Value!.AssertOrderDto(command, Expected);
    }
}
