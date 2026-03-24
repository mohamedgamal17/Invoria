using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Services;

namespace Invoria.Ordering.Tests.Fakes;

public sealed class CountingListCustomersCustomerService : ICustomerService
{
    public int ListCustomersByIdsCallCount { get; private set; }

    public void ResetCounters() => ListCustomersByIdsCallCount = 0;

    public Task<Result<CustomerDto>> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure<CustomerDto>(
            new NotFoundException($"Customer with ID {id} not found")));
    }

    public Task<Result<IReadOnlyList<CustomerDto>>> ListCustomersByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        ListCustomersByIdsCallCount++;

        var list = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .Select(id => new CustomerDto
            {
                Id = id,
                Name = SyntheticListCustomerService.NameForId(id)
            })
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyList<CustomerDto>>(list));
    }
}
