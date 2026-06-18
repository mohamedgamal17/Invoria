using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Contracts.Stock.Dtos;

namespace Invoria.Catalog.Contracts.Dtos
{
    public class ProductDto : AuditedEntityDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public StockDto Stock { get; set; } = new();
    }
}
