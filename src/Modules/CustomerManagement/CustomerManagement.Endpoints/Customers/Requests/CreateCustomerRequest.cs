using FluentValidation;

namespace Invoria.CustomerManagement.Endpoints.Customers.Requests
{
    public class CreateCustomerRequest : CustomerRequest
    {
    }

    public class CreateCustomerRequestValidator : CustomerRequestValidator<CreateCustomerRequest>
    {
    }
}

