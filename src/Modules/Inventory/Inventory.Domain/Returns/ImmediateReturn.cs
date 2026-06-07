using Ardalis.GuardClauses;

namespace Invoria.Inventory.Domain.Returns;

public class ImmediateReturn : Return
{
    public string AllocationId { get; private set; } = null!;

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
        : base(orderId, returnLines, ReturnType.Immediate)
    {
        Guard.Against.NullOrWhiteSpace(allocationId);
        Guard.Against.OutOfRange(
            allocationId.Length,
            nameof(allocationId),
            1,
            ImmediateReturnTableConsts.AllocationIdMaxLength);

        AllocationId = allocationId;
    }
}
