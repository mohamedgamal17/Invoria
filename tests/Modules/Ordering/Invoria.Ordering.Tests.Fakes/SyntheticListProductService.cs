using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;

namespace Invoria.Ordering.Tests.Fakes;

public sealed class SyntheticListProductService : IProductService
{
    public static string NameForId(string id) => $"Product-{id}";

    public Task<Result<ProductDto>> GetProductByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var dto = CreateDto(id);
        return Task.FromResult<Result<ProductDto>>(dto);
    }

    public Task<Result<IReadOnlyList<ProductDto>>> ListProductsByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        var list = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .Select(CreateDto)
            .ToList();

        return Task.FromResult<Result<IReadOnlyList<ProductDto>>>(list);
    }

    private static ProductDto CreateDto(string id)
    {
        return new ProductDto
        {
            Id = id,
            Name = NameForId(id),
            Code = null,
            Price = 9.99m
        };
    }
}
