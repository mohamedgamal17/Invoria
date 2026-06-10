using Ardalis.GuardClauses;

namespace Invoria.Inventory.Domain.Returns;

public class ImmediateReturn : Return
{
    public string AllocationId { get; private set; } = null!;

    public string OrderId { get; private set; } = null!;

    private ImmediateReturn()
    {
        Type = ReturnType.Immediate;
    }

    public static ImmediateReturn Create(
        string allocationId,
        string orderId,
        IEnumerable<ReturnLine> returnLines) =>
        new(allocationId, orderId, returnLines);

    private ImmediateReturn(
        string allocationId,
        string orderId,
        IEnumerable<ReturnLine> returnLines)
        : base(returnLines, ReturnType.Immediate)
    {
        Guard.Against.NullOrWhiteSpace(allocationId);
        Guard.Against.OutOfRange(
            allocationId.Length,
            nameof(allocationId),
            1,
            ImmediateReturnTableConsts.AllocationIdMaxLength);

        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(
            orderId.Length,
            nameof(orderId),
            1,
            ImmediateReturnTableConsts.OrderIdMaxLength);

        AllocationId = allocationId;
        OrderId = orderId;
    }
}
