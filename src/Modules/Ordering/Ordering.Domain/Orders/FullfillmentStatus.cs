namespace Invoria.Ordering.Domain.Orders
{
    public enum FullfillmentStatus
    {
        Pending = 5 ,
        Allocating = 10 ,
        Allocated = 15 ,
        OnHold = 20,
        Releasing = 25,
        Dispatched = 30,
        Cancelled = 35,
    }
}
