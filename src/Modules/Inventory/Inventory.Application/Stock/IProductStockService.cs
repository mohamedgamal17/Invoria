using Invoria.Inventory.Contracts.Stock.Dtos;

namespace Invoria.Inventory.Application.Stock;

public interface IProductStockService
{
    Task<StockDto> GetProductStock(string productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, StockDto>> GetListProductsStock(
        IReadOnlyCollection<string> productIds,
        CancellationToken cancellationToken = default);
}
