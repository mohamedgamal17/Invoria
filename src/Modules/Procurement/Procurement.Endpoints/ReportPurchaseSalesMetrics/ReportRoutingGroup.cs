using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Invoria.Procurement.Endpoints.ReportPurchaseSalesMetrics;

public sealed class ReportRoutingGroup : Group
{
    public ReportRoutingGroup()
    {
        Configure("report/purchase-orders/sales", ep =>
        {
            ep.Description(x =>
                x.WithTags("Reports")
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