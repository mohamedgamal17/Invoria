using FluentValidation;

namespace Invoria.Modules.Catalog.Endpoints.Products.Requests
{
    public class CreateProductRequest : ProductRequest
    {
    }

    public class CreateProductRequestValidator : ProductRequestValidator<CreateProductRequest> { }
}
