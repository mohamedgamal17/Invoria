using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Invoria.Reporting.Endpoints;

public sealed class ReportingRoutingGroup : Group
{
    public ReportingRoutingGroup()
    {
        Configure("reporting", ep =>
        {
            ep.Description(x =>
                x.WithTags("Reporting")
                    .Produces(StatusCodes.Status400BadRequest, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status401Unauthorized, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status403Forbidden, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status404NotFound, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status409Conflict, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status422UnprocessableEntity, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status500InternalServerError, typeof(ProblemDetails)));
        });
    }
}
