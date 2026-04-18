using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.CustomerManagement.Application.Customers.Factories;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.Customers.Queries.ListCustomers
{
    public class ListQueryHandler : IApplicatonRequestHandler<ListCustomerQuery, PagingDto<CustomerDto>>
    {
        private readonly ICustomerRepository<Customer> _customerRepository;
        private readonly ICustomerResponseFactory _customerResponseFactory;

        public ListQueryHandler(
            ICustomerRepository<Customer> customerRepository,
            ICustomerResponseFactory customerResponseFactory)
        {
            _customerRepository = customerRepository;
            _customerResponseFactory = customerResponseFactory;
        }

        public async Task<Result<PagingDto<CustomerDto>>> Handle(ListCustomerQuery request, CancellationToken cancellationToken)
        {
            var query = _customerRepository.AsQuerable();

            var nameTerm = request.Name?.Trim();
            if (!string.IsNullOrEmpty(nameTerm))
            {
                var normalizedNameTerm = nameTerm.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(normalizedNameTerm));
            }

            var result = await query.ToPaged(request.Skip, request.Length);

            var response = await _customerResponseFactory.PreparePagingDto(result);

            return response;
        }
    }
}

