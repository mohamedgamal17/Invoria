using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.CustomerManagement.Contracts.Dtos;

namespace Invoria.CustomerManagement.Application.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQuery : IQuery<CustomerDto>
    {
        public string Id { get; set; } = string.Empty;
    }
}

