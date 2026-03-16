using FluentValidation;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Endpoints.Products.Requests
{
    public class ProductRequest 
    {
        public string Name { get; set; }
        public string? Code { get; set; }
        public decimal Price { get; set; }
    }


    public class ProductRequestValidator<T> : AbstractValidator<T> 
        where T  : ProductRequest

    {
        public ProductRequestValidator()
        {
            RuleFor(x => x.Name)
                 .NotEmpty()
                 .MinimumLength(3)
                 .MaximumLength(ProductTableConsts.NameMaxLength);


            RuleFor(x => x.Code)
                .MinimumLength(3)
                .MaximumLength(ProductTableConsts.CodeMaxLength)
                .When(x => x.Code != null);


            RuleFor(x => x.Price).GreaterThan(0);
        }
    }

}
