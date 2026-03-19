using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.CustomerManagement.Application.Customers.Factories;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IApplicatonRequestHandler<CreateCustomerCommand, CustomerDto>
    {
        private readonly ICustomerRepository<Customer> _customerRepository;
        private readonly ICustomerResponseFactory _customerResponseFactory;

        public CreateCustomerCommandHandler(ICustomerRepository<Customer> customerRepository, ICustomerResponseFactory customerResponseFactory)
        {
            _customerRepository = customerRepository;
            _customerResponseFactory = customerResponseFactory;
        }

        public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = new Customer(request.Name);

            await _customerRepository.Add(customer);

            var dto = await _customerResponseFactory.PrepareDto(customer);

            return dto;
        }
    }
}

