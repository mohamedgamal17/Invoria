using Invoria.BuildingBlocks.Domain.Entities;
namespace Invoria.Procurement.Domain.PurchaseOrders;
public class PurchaseOrderSequence : AuditedEntity
{
    public int Year { get; private set; }
    public int CurrentValue { get; private set; }
    private PurchaseOrderSequence() { }
    public static PurchaseOrderSequence Create(string id, int year)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return new PurchaseOrderSequence { Id = id, Year = year, CurrentValue = 0 };
    }
    public int Increment() => ++CurrentValue;
}
