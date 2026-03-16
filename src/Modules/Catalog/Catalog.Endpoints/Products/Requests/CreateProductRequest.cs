using FluentValidation;

namespace Invoria.Catalog.Endpoints.Products.Requests
{
    public class CreateProductRequest : ProductRequest
    {
    }

    public class CreateProductRequestValidator : ProductRequestValidator<CreateProductRequest> { }
}
