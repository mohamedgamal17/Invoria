using Invoria.BuildingBlocks.Domain.Dtos;

namespace Invoria.CustomerManagement.Contracts.Dtos
{
    public class CustomerDto : AuditedEntityDto
    {
        public string Name { get; set; } = default!;
    }
}
