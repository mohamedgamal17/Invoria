using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Invoria.Procurement.Endpoints.PurchaseOrders;

public sealed class PurchaseOrderRoutingGroup : Group
{
    public PurchaseOrderRoutingGroup()
    {
        Configure("purchase-orders", ep =>
        {
            ep.Description(x =>
                x.WithTags("PurchaseOrders")
                    .Produces(StatusCodes.Status401Unauthorized, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status403Forbidden, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status400BadRequest, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status500InternalServerError, typeof(ProblemDetails)));
        });
    }
}
