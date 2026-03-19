using Invoria.BuildingBlocks.Application.Factories;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.Customers.Factories
{
    public class CustomerResponseFactory : ResponseFactory<Customer, CustomerDto>, ICustomerResponseFactory
    {
        public override Task<CustomerDto> PrepareDto(Customer view)
        {
            var dto = new CustomerDto
            {
                Id = view.Id,
                Name = view.Name
            };

            MapAudited(view, dto);

            return Task.FromResult(dto);
        }
    }
}

