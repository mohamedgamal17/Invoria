using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.CustomerManagement.Contracts.Dtos;

namespace Invoria.CustomerManagement.Application.Customers.Queries.ListCustomers
{
    public class ListCustomerQuery : PagingParams, IQuery<PagingDto<CustomerDto>>
    {
        public string? Name { get; set; }
    }
}

