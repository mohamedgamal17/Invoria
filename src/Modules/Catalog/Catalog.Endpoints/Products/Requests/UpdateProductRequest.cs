using FluentValidation;

namespace Invoria.Modules.Catalog.Endpoints.Products.Requests
{
    public class UpdateProductRequest : ProductRequest
    {
        public string Id { get; set; }
    }

    public class UpdateProductRequestValidator : ProductRequestValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            
        }
    }


}
