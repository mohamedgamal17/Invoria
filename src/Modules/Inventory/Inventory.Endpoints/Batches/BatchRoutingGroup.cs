using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Invoria.Inventory.Endpoints.Batches;

public class BatchRoutingGroup : Group
{
    public BatchRoutingGroup()
    {
        Configure("batches", ep =>
        {
            ep.Description(x =>
                x.WithTags("Batches")
                    .Produces(StatusCodes.Status401Unauthorized, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status403Forbidden, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status400BadRequest, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status500InternalServerError, typeof(ProblemDetails)));
        });
    }
}
