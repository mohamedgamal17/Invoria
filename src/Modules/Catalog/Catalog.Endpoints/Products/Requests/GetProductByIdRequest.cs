using FluentValidation;

namespace Invoria.Catalog.Endpoints.Products.Requests
{
    public class GetProductByIdRequest
    {
        public string Id { get; set; } = string.Empty;
    }

    public class GetProductByIdRequestValidator : AbstractValidator<GetProductByIdRequest>
    {
        public GetProductByIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}
