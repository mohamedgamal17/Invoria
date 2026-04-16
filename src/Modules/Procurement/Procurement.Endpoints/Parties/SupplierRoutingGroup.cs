using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Invoria.Procurement.Endpoints.Parties;

public sealed class SupplierRoutingGroup : Group
{
    public SupplierRoutingGroup()
    {
        Configure("suppliers", ep =>
        {
            ep.Description(x =>
                x.WithTags("Suppliers")
                    .Produces(StatusCodes.Status401Unauthorized, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status403Forbidden, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status400BadRequest, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status500InternalServerError, typeof(ProblemDetails)));
        });
    }
}

