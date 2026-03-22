using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class ListOrdersRequest : PagingParams
{
    public string? OrderNumber { get; set; }

    public bool IncludeOrderItems { get; set; }
}

public class ListOrdersRequestValidator : AbstractValidator<ListOrdersRequest>
{
    public ListOrdersRequestValidator()
    {
        Include(new PagingParamasValidator<ListOrdersRequest>());
    }
}
