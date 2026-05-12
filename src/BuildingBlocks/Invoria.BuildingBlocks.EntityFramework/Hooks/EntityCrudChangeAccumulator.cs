namespace Invoria.BuildingBlocks.EntityFramework.Hooks;

public sealed class EntityCrudChangeAccumulator : IEntityCrudChangeAccumulator
{
    private readonly List<(object Entity, EntityCrudKind Kind)> _pending = new();

    public void StartBatch()
    {
        _pending.Clear();
    }

    public void Enqueue(object aggregateRoot, EntityCrudKind kind)
    {
        ArgumentNullException.ThrowIfNull(aggregateRoot);
        _pending.Add((aggregateRoot, kind));
    }

    public IReadOnlyList<(object Entity, EntityCrudKind Kind)> Drain()
    {
        var snapshot = _pending.ToArray();
        _pending.Clear();
        return snapshot;
    }
}
