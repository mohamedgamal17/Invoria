namespace Invoria.BuildingBlocks.EntityFramework.Hooks;

public interface IEntityCrudChangeAccumulator
{
    void StartBatch();

    void Enqueue(object aggregateRoot, EntityCrudKind kind);

    IReadOnlyList<(object Entity, EntityCrudKind Kind)> Drain();
}
