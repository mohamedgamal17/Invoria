using FluentValidation;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Endpoints.Customers.Requests
{
    public class CustomerRequest
    {
        public string Name { get; set; } = default!;
    }

    public class CustomerRequestValidator<T> : AbstractValidator<T>
        where T : CustomerRequest
    {
        public CustomerRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(CustomerTableConsts.NameMaxLength);
        }
    }
}

