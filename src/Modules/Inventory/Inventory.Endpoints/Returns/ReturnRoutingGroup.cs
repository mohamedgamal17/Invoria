using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Invoria.Inventory.Endpoints.Returns;

public class ReturnRoutingGroup : Group
{
    public ReturnRoutingGroup()
    {
        Configure("returns", ep =>
        {
            ep.Description(x => x
                .WithTags("Returns")
                .Produces(StatusCodes.Status400BadRequest, typeof(ProblemDetails))
                .Produces(StatusCodes.Status401Unauthorized, typeof(ProblemDetails))
                .Produces(StatusCodes.Status403Forbidden, typeof(ProblemDetails))
                .Produces(StatusCodes.Status404NotFound, typeof(ProblemDetails))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ProblemDetails)));
        });
    }
}
