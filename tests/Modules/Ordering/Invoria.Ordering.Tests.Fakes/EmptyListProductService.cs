using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;

namespace Invoria.Ordering.Tests.Fakes;

public sealed class EmptyListProductService : IProductService
{
    public Task<Result<ProductDto>> GetProductByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure<ProductDto>(
            new NotFoundException($"Product with ID {id} not found")));
    }

    public Task<Result<IReadOnlyList<ProductDto>>> ListProductsByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductDto> empty = Array.Empty<ProductDto>();
        return Task.FromResult(Result.Success(empty));
    }
}
