using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Inventory.Domain.Returns;

public abstract class Return : AuditedAggregateRoot
{
    private readonly List<ReturnLine> _returnLines = new();

    public string OrderId { get; private set; } = null!;

    public IReadOnlyCollection<ReturnLine> ReturnLines => _returnLines.AsReadOnly();

    protected Return()
    {
    }

    protected Return(string orderId, IEnumerable<ReturnLine> returnLines)
    {
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, ReturnTableConsts.OrderIdMaxLength);

        var lines = returnLines?.ToList() ?? [];
        Guard.Against.NullOrEmpty(lines);

        Id = Guid.NewGuid().ToString();
        OrderId = orderId;

        foreach (var line in lines)
        {
            Guard.Against.Null(line);
            line.AttachToReturn(Id!);
            _returnLines.Add(line);
        }
    }
}
