using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Invoria.Ordering.Endpoints.Orders;

public class OrderRoutingGroup : Group
{
    public OrderRoutingGroup()
    {
        Configure("orders", ep =>
        {
            ep.Description(x =>
                x.WithTags("Orders")
                    .Produces(StatusCodes.Status401Unauthorized, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status403Forbidden, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status404NotFound, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status400BadRequest, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status500InternalServerError, typeof(ProblemDetails)));
        });
    }
}
