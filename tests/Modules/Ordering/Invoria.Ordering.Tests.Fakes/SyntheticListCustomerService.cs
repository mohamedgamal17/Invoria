using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Services;

namespace Invoria.Ordering.Tests.Fakes;

public sealed class SyntheticListCustomerService : ICustomerService
{
    public static string NameForId(string id) => $"Customer-{id}";

    public Task<Result<CustomerDto>> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure<CustomerDto>(
            new NotFoundException($"Customer with ID {id} not found")));
    }

    public Task<Result<IReadOnlyList<CustomerDto>>> ListCustomersByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        var list = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .Select(id => new CustomerDto
            {
                Id = id,
                Name = NameForId(id)
            })
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyList<CustomerDto>>(list));
    }
}
