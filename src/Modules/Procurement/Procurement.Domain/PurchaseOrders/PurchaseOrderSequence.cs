using Invoria.BuildingBlocks.Domain.Entities;
namespace Invoria.Procurement.Domain.PurchaseOrders;
public class PurchaseOrderSequence : AuditedEntity
{
    public int Year { get; private set; }
    public int Month { get; private set; }
    public int Day { get; private set; }
    public int CurrentValue { get; private set; }
    private PurchaseOrderSequence() { }
    public static PurchaseOrderSequence Create(int year, int month, int day)
    {
        return new PurchaseOrderSequence
        {
            Id = $"{year:D4}{month:D2}{day:D2}",
            Year = year,
            Month = month,
            Day = day,
            CurrentValue = 0
        };
    }
    public int Increment() => ++CurrentValue;
}
