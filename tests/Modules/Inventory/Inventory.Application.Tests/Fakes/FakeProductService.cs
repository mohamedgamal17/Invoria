using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;

namespace Invoria.Inventory.Application.Tests.Fakes;

public class FakeProductService : IProductService
{
    public Task<Result<ProductDto>> GetProductByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var dto = new ProductDto
        {
            Id = id ?? "",
            Name = $"Product_{id}",
            Price = 10m
        };

        return Task.FromResult(Result.Success(dto));
    }

    public Task<Result<IReadOnlyList<ProductDto>>> ListProductsByIdsAsync(
        IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var list = (ids ?? [])
            .Select(id => new ProductDto
            {
                Id = id ?? "",
                Name = $"Product_{id}",
                Price = 10m
            })
            .ToList() as IReadOnlyList<ProductDto>;

        return Task.FromResult(Result.Success(list));
    }
}
