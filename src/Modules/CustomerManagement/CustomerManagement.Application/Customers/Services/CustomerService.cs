using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.CustomerManagement.Application.Customers.Factories;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Services;
using Invoria.CustomerManagement.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Invoria.CustomerManagement.Application.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository<Customer> _customerRepository;
    private readonly ICustomerResponseFactory _customerResponseFactory;

    public CustomerService(
        ICustomerRepository<Customer> customerRepository,
        ICustomerResponseFactory customerResponseFactory)
    {
        _customerRepository = customerRepository;
        _customerResponseFactory = customerResponseFactory;
    }

    public async Task<Result<CustomerDto>> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.SingleOrDefault(c => c.Id == id, cancellationToken);
        if (customer == null)
        {
            return Result.Failure<CustomerDto>(new NotFoundException($"Customer with ID {id} not found"));
        }

        var dto = await _customerResponseFactory.PrepareDto(customer);

        return dto;
    }

    public async Task<Result<IReadOnlyList<CustomerDto>>> ListCustomersByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (idList.Count == 0)
        {
            return new List<CustomerDto>();
        }

        var customers = await _customerRepository
            .AsQuerable()
            .Where(c => idList.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var dtos = await _customerResponseFactory.PrepareListDto(customers);

        return dtos;
    }
}
