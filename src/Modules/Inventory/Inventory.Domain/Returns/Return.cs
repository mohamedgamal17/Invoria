using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Contracts.Returns.Enums;

namespace Invoria.Inventory.Domain.Returns;

public abstract class Return : AuditedAggregateRoot
{
    private readonly List<ReturnLine> _returnLines = new();

    public ReturnType Type { get; protected set; }

    public ReturnStatus Status { get; private set; }

    public IReadOnlyCollection<ReturnLine> ReturnLines => _returnLines.AsReadOnly();

    protected Return()
    {
    }

    protected Return(IEnumerable<ReturnLine> returnLines, ReturnType type)
    {
        var lines = returnLines?.ToList() ?? [];
        Guard.Against.NullOrEmpty(lines);

        Id = Guid.NewGuid().ToString();
        Type = type;
        Status = ReturnStatus.Pending;

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
