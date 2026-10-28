using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Financial.Contracts.Payables.Dtos;

namespace Invoria.Financial.Application.Payables.Queries.GetPayableByPurchaseOrderId;

public class GetPayableByPurchaseOrderIdQuery : IQuery<PayableDto>
{
    public string PurchaseOrderId { get; set; } = string.Empty;
}
