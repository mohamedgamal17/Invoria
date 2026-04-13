using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Invoria.Catalog.Endpoints.Products
{
    public class ProductRoutingGroup : Group
    {
        public ProductRoutingGroup()
        {
            Configure("products", ep =>
            {

                ep.Description(x =>
                    x
                    .WithTags("Products")
                    .Produces(StatusCodes.Status401Unauthorized, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status403Forbidden, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status400BadRequest, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status404NotFound, typeof(ProblemDetails))
                    .Produces(StatusCodes.Status500InternalServerError, typeof(ProblemDetails)));


            });
        }
    }
}
