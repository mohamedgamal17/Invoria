using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;

namespace Invoria.Ordering.Tests.Fakes;

public sealed class CountingListProductsProductService : IProductService
{
    public int ListProductsByIdsCallCount { get; private set; }

    public IReadOnlyList<string> LastRequestedProductIds { get; private set; } = [];

    public void ResetCounters()
    {
        ListProductsByIdsCallCount = 0;
        LastRequestedProductIds = [];
    }

    public Task<Result<ProductDto>> GetProductByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure<ProductDto>(
            new NotFoundException($"Product with ID {id} not found")));
    }

    public Task<Result<IReadOnlyList<ProductDto>>> ListProductsByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        ListProductsByIdsCallCount++;

        LastRequestedProductIds = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var list = LastRequestedProductIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .Select(id => new ProductDto
            {
                Id = id,
                Name = SyntheticListProductService.NameForId(id),
                Price = 9.99m
            })
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyList<ProductDto>>(list));
    }
}
