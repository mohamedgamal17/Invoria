using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.CustomerManagement.Application.Customers.Factories;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandHandler : IApplicatonRequestHandler<UpdateCustomerCommand, CustomerDto>
    {
        private readonly ICustomerRepository<Customer> _customerRepository;
        private readonly ICustomerResponseFactory _customerResponseFactory;

        public UpdateCustomerCommandHandler(ICustomerRepository<Customer> customerRepository, ICustomerResponseFactory customerResponseFactory)
        {
            _customerRepository = customerRepository;
            _customerResponseFactory = customerResponseFactory;
        }

        public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.SingleOrDefault(c => c.Id == request.Id, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<CustomerDto>(new NotFoundException($"Customer with ID {request.Id} not found"));
            }

            customer.Update(request.Name);

            await _customerRepository.Update(customer, cancellationToken);

            var dto = await _customerResponseFactory.PrepareDto(customer);

            return dto;
        }
    }
}

