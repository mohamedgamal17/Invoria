using Invoria.Inventory.Contracts.Stock.Dtos;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Stock;

public class ProductStockService : IProductStockService
{
    private readonly IInventoryRepository<Batch> _batchRepository;

    public ProductStockService(IInventoryRepository<Batch> batchRepository)
    {
        _batchRepository = batchRepository;
    }

    public async Task<StockDto> GetProductStock(string productId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        var key = productId.Trim();
        var map = await GetListProductsStock(new[] { key }, cancellationToken).ConfigureAwait(false);
        return map[key];
    }

    public async Task<IReadOnlyDictionary<string, StockDto>> GetListProductsStock(
        IReadOnlyCollection<string> productIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(productIds);

        if (productIds.Count == 0)
        {
            return new Dictionary<string, StockDto>();
        }

        var normalizedIds = NormalizeProductIds(productIds);
        if (normalizedIds.Count == 0)
        {
            return new Dictionary<string, StockDto>();
        }

        var aggregates = await _batchRepository
            .AsQuerable()
            .Where(b => b.State == BatchState.Active && normalizedIds.Contains(b.ProductId))
            .GroupBy(b => b.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                ActualQuantity = g.Sum(x => x.Quantity),
                ReservedQuantity = g.Sum(x => x.ReservedQuantity),
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var lookup = aggregates.ToDictionary(x => x.ProductId, x => new StockDto
        {
            ActualQuantity = x.ActualQuantity,
            ReservedQuantity = x.ReservedQuantity,
        });

        var result = new Dictionary<string, StockDto>(StringComparer.Ordinal);
        foreach (var id in normalizedIds)
        {
            result[id] = lookup.TryGetValue(id, out var dto)
                ? dto
                : new StockDto();
        }

        return result;
    }

    private static List<string> NormalizeProductIds(IReadOnlyCollection<string> productIds)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var list = new List<string>();
        foreach (var id in productIds)
        {
            var trimmed = id?.Trim() ?? "";
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (seen.Add(trimmed))
            {
                list.Add(trimmed);
            }
        }

        return list;
    }
}
