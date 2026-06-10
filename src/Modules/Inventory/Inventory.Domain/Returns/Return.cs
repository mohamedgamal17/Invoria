using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Contracts.Returns.Enums;

namespace Invoria.Inventory.Domain.Returns;

public abstract class Return : AuditedAggregateRoot
{
    private readonly List<ReturnLine> _returnLines = new();

    public ReturnType Type { get; protected set; }

    public ReturnStatus Status { get; private set; }

    public string OrderId { get; private set; } = null!;

    public IReadOnlyCollection<ReturnLine> ReturnLines => _returnLines.AsReadOnly();

    protected Return()
    {
    }

    protected Return(string orderId, IEnumerable<ReturnLine> returnLines, ReturnType type)
    {
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, ReturnTableConsts.OrderIdMaxLength);

        var lines = returnLines?.ToList() ?? [];
        Guard.Against.NullOrEmpty(lines);

        Id = Guid.NewGuid().ToString();
        Type = type;
        Status = ReturnStatus.Pending;
        OrderId = orderId;

        foreach (var line in lines)
        {
            Guard.Against.Null(line);
            line.AttachToReturn(Id!);
            _returnLines.Add(line);
        }
    }

    public void Approve()
    {
        if (Status != ReturnStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Return {Id} must be in {ReturnStatus.Pending} state to approve.");
        }

        Status = ReturnStatus.Approved;
    }

    public void Reject()
    {
        if (Status != ReturnStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Return {Id} must be in {ReturnStatus.Pending} state to reject.");
        }

        Status = ReturnStatus.Rejected;
    }

    public void Complete()
    {
        if (Status != ReturnStatus.Approved)
        {
            throw new InvalidOperationException(
                $"Return {Id} must be in {ReturnStatus.Approved} state to complete.");
        }

        Status = ReturnStatus.Completed;
    }
}
