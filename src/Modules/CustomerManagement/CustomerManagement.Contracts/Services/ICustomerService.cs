using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.CustomerManagement.Contracts.Dtos;

namespace Invoria.CustomerManagement.Contracts.Services;

public interface ICustomerService
{
    Task<Result<CustomerDto>> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CustomerDto>>> ListCustomersByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default);
}
