using FluentValidation;

namespace Invoria.CustomerManagement.Endpoints.Customers.Requests
{
    public class GetCustomerByIdRequest
    {
        public string Id { get; set; } = string.Empty;
    }

    public class GetCustomerByIdRequestValidator : AbstractValidator<GetCustomerByIdRequest>
    {
        public GetCustomerByIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

