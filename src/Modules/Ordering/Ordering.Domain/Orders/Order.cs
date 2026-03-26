using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Ordering.Domain.Orders
{
    public class Order : AuditedAggregateRoot
    {
        public string OrderNumber { get;private set; }
        public string CustomerId { get; private set; }
        public List<OrderItem> Items { get; private set; } 
        public OrderStatus Status { get; private set; }


        private Order()
        {
            
        }

        public Order(string orderNumber, string customerId)
        {
            OrderNumber = orderNumber;
            CustomerId = customerId;
            Items = new List<OrderItem>();
            Status = OrderStatus.Pending;
        }


        public void UpdateItems(List<OrderItem> items)
        {
            Guard.Against.Null(items);

            if(items.Count <= 0)
            {
                throw new InvalidOperationException("Order items must have one or more item.");
            }

            if (Status != OrderStatus.Pending && Status != OrderStatus.Reopened)
            {
                throw new InvalidOperationException(
                    "Order items can only be updated when the order is Pending or Reopened.");
            }

            Items = items;
        }

        public void Accept()
        {
            if (Status != OrderStatus.Pending && Status != OrderStatus.Reopened)
            {
                throw new InvalidOperationException(
                    "Order can only be accepted when it is Pending or Reopened.");
            }

            Status = OrderStatus.Accepted;
        }

        public void Reopen()
        {
            if (Status != OrderStatus.Accepted)
            {
                throw new InvalidOperationException(
                    "Order can only be reopened when it is Accepted.");
            }

            Status = OrderStatus.Reopened;
        }

        public void Cancel()
        {
            if (Status != OrderStatus.Pending && Status != OrderStatus.Reopened)
            {
                throw new InvalidOperationException(
                    "Order can only be cancelled when the order is Pending or Reopened.");
            }

            Status = OrderStatus.Cancelled;
        }

        public void Refuse()
        {
            if (Status != OrderStatus.Accepted && Status != OrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Order can only be refused when it is Accepted or Completed.");
            }

            Status = OrderStatus.Refused;
        }

        public void Complete()
        {
            if (Status != OrderStatus.Accepted)
            {
                throw new InvalidOperationException(
                    "Order can only be completed when it is Accepted.");
            }

            Status = OrderStatus.Completed;
        }
    }
}
