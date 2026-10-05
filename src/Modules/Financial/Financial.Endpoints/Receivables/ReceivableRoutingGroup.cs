using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Invoria.Financial.Endpoints.Receivables;

public class ReceivableRoutingGroup : Group
{
    public ReceivableRoutingGroup()
    {
        Configure("receivables", ep =>
        {
            ep.Description(x => x
                .WithTags("Receivables")
                .Produces(StatusCodes.Status400BadRequest, typeof(ProblemDetails))
                .Produces(StatusCodes.Status401Unauthorized, typeof(ProblemDetails))
                .Produces(StatusCodes.Status403Forbidden, typeof(ProblemDetails))
                .Produces(StatusCodes.Status404NotFound, typeof(ProblemDetails))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ProblemDetails)));
        });
    }
}
