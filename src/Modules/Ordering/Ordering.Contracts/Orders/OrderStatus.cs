namespace Invoria.Ordering.Contracts.Orders
{
    public enum OrderStatus
    {
        Pending = 5 ,

        Accepted = 10,

        Shipped = 15,

        Completed = 20,

        Cancelled = 25,

        Reopened = 30,

        Refused = 35
    }
}
