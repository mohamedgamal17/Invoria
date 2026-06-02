namespace Invoria.Ordering.Contracts.Orders
{
    public enum OrderStatus
    {
        Pending = 5 ,

        Processing = 10,

        Revision = 15,

        Completed = 20,
        
        Cancelled = 25
    }
}
