using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Invoria.Financial.Endpoints.Payables;

public class PayableRoutingGroup : Group
{
    public PayableRoutingGroup()
    {
        Configure("payables", ep =>
        {
            ep.Description(x => x
                .WithTags("Payables")
                .Produces(StatusCodes.Status400BadRequest, typeof(ProblemDetails))
                .Produces(StatusCodes.Status401Unauthorized, typeof(ProblemDetails))
                .Produces(StatusCodes.Status403Forbidden, typeof(ProblemDetails))
                .Produces(StatusCodes.Status404NotFound, typeof(ProblemDetails))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ProblemDetails)));
        });
    }
}
