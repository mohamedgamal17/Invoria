using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Catalog.Contracts.Dtos;

namespace Invoria.Catalog.Contracts.Services
{
    public interface IProductService
    {
        Task<Result<ProductDto>> GetProductByIdAsync(string id, CancellationToken cancellationToken = default);

        Task<Result<IReadOnlyList<ProductDto>>> ListProductsByIdsAsync(
            IEnumerable<string> ids,
            CancellationToken cancellationToken = default);
    }
}
