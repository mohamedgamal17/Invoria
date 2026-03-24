using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Services;

namespace Invoria.Ordering.Tests.Fakes;

public sealed class EmptyListCustomerService : ICustomerService
{
    public Task<Result<CustomerDto>> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure<CustomerDto>(
            new NotFoundException($"Customer with ID {id} not found")));
    }

    public Task<Result<IReadOnlyList<CustomerDto>>> ListCustomersByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CustomerDto> empty = Array.Empty<CustomerDto>();
        return Task.FromResult(Result.Success(empty));
    }
}
