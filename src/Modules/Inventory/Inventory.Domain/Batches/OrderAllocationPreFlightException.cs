using Invoria.BuildingBlocks.Domain.Exceptions;

namespace Invoria.Inventory.Domain.Batches;

public sealed record OrderAllocationPreFlightError(
    string ProductId,
    int RequestedQuantity,
    int AvailableQuantity)
{
    public string Message =>
        $"Insufficient stock for product {ProductId}: requested {RequestedQuantity}, available {AvailableQuantity}.";
}

public sealed class OrderAllocationPreFlightException : ApplicationExceptionBase
{
    public const string DefaultCode = "order_allocation_pre_flight";

    public IReadOnlyCollection<OrderAllocationPreFlightError> Errors { get; }

    public OrderAllocationPreFlightException(IReadOnlyCollection<OrderAllocationPreFlightError> errors)
        : base(DefaultCode, string.Join(Environment.NewLine, errors.Select(e => e.Message)))
    {
        Errors = errors;
    }
}
