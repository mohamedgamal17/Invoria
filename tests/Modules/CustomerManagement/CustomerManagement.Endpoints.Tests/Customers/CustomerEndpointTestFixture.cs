using Invoria.CustomerManagement.Domain.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Endpoints.Tests.Customers
{
    public class CustomerEndpointTestFixture : CustomerTestFixture
    {
        protected ICustomerRepository<Customer> CustomerRepository { get; private set; } = null!;

        public CustomerEndpointTestFixture()
        {
            CustomerRepository = Scope.ServiceProvider.GetRequiredService<ICustomerRepository<Customer>>();
        }
    }
}

