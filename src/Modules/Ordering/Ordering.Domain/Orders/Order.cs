using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Domain.Orders.Events;

namespace Invoria.Ordering.Domain.Orders
{
    public class Order : AuditedAggregateRoot
    {
        public string OrderNumber { get;private set; }
        public string CustomerId { get; private set; }
        public List<OrderItem> Items { get; private set; } 
        public OrderStatus Status { get; private set; }
        public FullfillmentStatus FullfillmentStatus { get; set; }
        private Order()
        {
            
        }

        public Order(string orderNumber, string customerId)
        {
            OrderNumber = orderNumber;
            CustomerId = customerId;
            Items = new List<OrderItem>();
            Status = OrderStatus.Pending;
            FullfillmentStatus = FullfillmentStatus.Pending;
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

            if (FullfillmentStatus != FullfillmentStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Order can only be accepted when fulfillment is Pending.");
            }

            Status = OrderStatus.Accepted;
            FullfillmentStatus = FullfillmentStatus.Allocating;
            AddDomainEvent(new OrderAcceptedDomainEvent(Id, OrderNumber, CustomerId));
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

        /// <summary>
        /// Cancels the order when inventory allocation fails after the order was accepted.
        /// </summary>
        public void CancelDueToAllocationFailure(string reason)
        {
            Guard.Against.NullOrWhiteSpace(reason);

            if (Status != OrderStatus.Accepted || FullfillmentStatus != FullfillmentStatus.Allocating)
            {
                throw new InvalidOperationException(
                    "Order can only be cancelled due to allocation failure when it is Accepted and allocating inventory.");
            }

            Status = OrderStatus.Cancelled;
            FullfillmentStatus = FullfillmentStatus.Pending;
        }

        /// <summary>
        /// Confirms inventory allocation succeeded after the order was accepted.
        /// Idempotent when fulfillment is already <see cref="FullfillmentStatus.Allocated"/>.
        /// </summary>
        public void MarkInventoryAllocated()
        {
            if (FullfillmentStatus == FullfillmentStatus.Allocated)
            {
                return;
            }

            if (Status != OrderStatus.Accepted || FullfillmentStatus != FullfillmentStatus.Allocating)
            {
                throw new InvalidOperationException(
                    "Order can only be marked inventory allocated when it is Accepted and allocating inventory.");
            }

            FullfillmentStatus = FullfillmentStatus.Allocated;
        }

        /// <summary>
        /// Marks the order as dispatched after inventory is allocated.
        /// Idempotent when fulfillment is already <see cref="FullfillmentStatus.Dispatched"/>.
        /// </summary>
        public void MarkDispatched()
        {
            if (FullfillmentStatus == FullfillmentStatus.Dispatched)
            {
                return;
            }

            if (Status != OrderStatus.Accepted || FullfillmentStatus != FullfillmentStatus.Allocated)
            {
                throw new InvalidOperationException(
                    "Order can only be marked dispatched when it is Accepted and inventory is allocated.");
            }

            FullfillmentStatus = FullfillmentStatus.Dispatched;

            var lines = Items
                .Select(i => new OrderDispatchedLine(i.Id, i.ProductId, i.Quantity))
                .ToList();

            AddDomainEvent(new OrderDispatchedDomainEvent(Id, OrderNumber, CustomerId, lines));
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
