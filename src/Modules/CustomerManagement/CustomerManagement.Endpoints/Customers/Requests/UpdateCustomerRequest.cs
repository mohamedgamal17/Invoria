using FluentValidation;

namespace Invoria.CustomerManagement.Endpoints.Customers.Requests
{
    public class UpdateCustomerRequest : CustomerRequest
    {
        public string Id { get; set; } = default!;
    }

    public class UpdateCustomerRequestValidator : CustomerRequestValidator<UpdateCustomerRequest>
    {
    }
}

