using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Modules.Catalog.Contracts.Dtos
{
    public class ProductDto : AuditedEntityDto
    {
        public string Name { get; set; }
        public string? Code { get; set; }
        public decimal Price { get; set; }
    }
}
