using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
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
            var lines = Items
                .Select(i => new OrderAcceptedLine(i.Id, i.ProductId, i.Quantity))
                .ToList();
            AddDomainEvent(new OrderAcceptedDomainEvent(Id, OrderNumber, CustomerId, lines));
        }

        /// <summary>
        /// Reopens an accepted order. When fulfillment is <see cref="FullfillmentStatus.Pending"/>, moves to
        /// <see cref="FullfillmentStatus.OnHold"/> and <see cref="OrderStatus.Reopened"/> immediately.
        /// When fulfillment is <see cref="FullfillmentStatus.Allocated"/> or <see cref="FullfillmentStatus.Allocating"/>,
        /// moves to <see cref="FullfillmentStatus.Releasing"/> and raises <see cref="OrderReopenReleaseRequestedDomainEvent"/>;
        /// call <see cref="CompleteReopenAfterInventoryReleased"/> after inventory has released allocations (or none yet).
        /// Cannot reopen after <see cref="FullfillmentStatus.Dispatched"/> because the order has left inventory and is shipping.
        /// </summary>
        public void Reopen()
        {
            if (Status != OrderStatus.Accepted)
            {
                throw new InvalidOperationException(
                    "Order can only be reopened when it is Accepted.");
            }

            switch (FullfillmentStatus)
            {
                case FullfillmentStatus.Pending:
                    FullfillmentStatus = FullfillmentStatus.OnHold;
                    Status = OrderStatus.Reopened;
                    return;
                case FullfillmentStatus.Allocating:
                case FullfillmentStatus.Allocated:
                    FullfillmentStatus = FullfillmentStatus.Releasing;
                    var lines = Items
                        .Select(i => new OrderDispatchedLine(i.Id, i.ProductId, i.Quantity))
                        .ToList();
                    AddDomainEvent(new OrderReopenReleaseRequestedDomainEvent(Id, OrderNumber, CustomerId, lines));
                    return;
                case FullfillmentStatus.Dispatched:
                    throw new InvalidOperationException(
                        "Order cannot be reopened after dispatch; fulfillment has left inventory and is shipping to the customer.");
                default:
                    throw new InvalidOperationException(
                        "Order can only be reopened when fulfillment is Pending, allocating, or allocated for release.");
            }
        }

        /// <summary>
        /// Completes reopen after <see cref="FullfillmentStatus.Releasing"/> and inventory release; sets
        /// <see cref="FullfillmentStatus.OnHold"/> and <see cref="OrderStatus.Reopened"/>.
        /// </summary>
        public void CompleteReopenAfterInventoryReleased()
        {
            if (Status != OrderStatus.Accepted || FullfillmentStatus != FullfillmentStatus.Releasing)
            {
                throw new InvalidOperationException(
                    "Order can only complete reopen after inventory release when it is Accepted and releasing inventory.");
            }

            FullfillmentStatus = FullfillmentStatus.OnHold;
            Status = OrderStatus.Reopened;
        }

        public void Cancel()
        {
            if (Status != OrderStatus.Pending
                && Status != OrderStatus.Accepted
                && Status != OrderStatus.Reopened)
            {
                throw new InvalidOperationException(
                    "Order can only be cancelled when the order is Pending, Accepted, or Reopened.");
            }

            if (FullfillmentStatus != FullfillmentStatus.Pending
                && FullfillmentStatus != FullfillmentStatus.OnHold
                && FullfillmentStatus != FullfillmentStatus.Allocated)
            {
                throw new InvalidOperationException(
                    "Order can only be cancelled when fulfillment is Pending, On Hold, or Allocated.");
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

        /// <summary>
        /// Refuses the order. When fulfillment is <see cref="FullfillmentStatus.Allocated"/>, moves to
        /// <see cref="FullfillmentStatus.Releasing"/> and raises <see cref="OrderRefusalReleaseRequestedDomainEvent"/>.
        /// Otherwise when accepted (no allocated stock to release, or dispatched), sets terminal fulfillment or
        /// leaves dispatch as-is and raises <see cref="OrderRefusedDomainEvent"/>. Completed orders only raise
        /// <see cref="OrderRefusedDomainEvent"/>.
        /// </summary>
        public void Refuse()
        {
            if (Status != OrderStatus.Accepted && Status != OrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Order can only be refused when it is Accepted or Completed.");
            }

            if (Status == OrderStatus.Completed)
            {
                Status = OrderStatus.Refused;
                AddDomainEvent(new OrderRefusedDomainEvent(Id, OrderNumber, CustomerId));
                return;
            }

            switch (FullfillmentStatus)
            {
                case FullfillmentStatus.Allocated:
                    Status = OrderStatus.Refused;
                    FullfillmentStatus = FullfillmentStatus.Releasing;
                    var refusalLines = Items
                        .Select(i => new OrderDispatchedLine(i.Id, i.ProductId, i.Quantity))
                        .ToList();
                    AddDomainEvent(new OrderRefusalReleaseRequestedDomainEvent(Id, OrderNumber, CustomerId, refusalLines));
                    return;
                case FullfillmentStatus.Releasing:
                    throw new InvalidOperationException(
                        "Order cannot be refused while inventory allocations are being released; complete or cancel the reopen flow first.");
                case FullfillmentStatus.Dispatched:
                    Status = OrderStatus.Refused;
                    AddDomainEvent(new OrderRefusedDomainEvent(Id, OrderNumber, CustomerId));
                    return;
                case FullfillmentStatus.Allocating:
                case FullfillmentStatus.Pending:
                case FullfillmentStatus.OnHold:
                    Status = OrderStatus.Refused;
                    FullfillmentStatus = FullfillmentStatus.Cancelled;
                    AddDomainEvent(new OrderRefusedDomainEvent(Id, OrderNumber, CustomerId));
                    return;
                case FullfillmentStatus.Cancelled:
                    throw new InvalidOperationException("Order fulfillment is already cancelled.");
                default:
                    throw new InvalidOperationException(
                        "Order cannot be refused in the current fulfillment state.");
            }
        }

        /// <summary>
        /// Completes refusal after <see cref="FullfillmentStatus.Releasing"/> and inventory release; sets
        /// <see cref="FullfillmentStatus.Cancelled"/>; <see cref="OrderStatus"/> remains <see cref="OrderStatus.Refused"/>.
        /// </summary>
        public void CompleteRefusalAfterInventoryReleased()
        {
            if (Status != OrderStatus.Refused || FullfillmentStatus != FullfillmentStatus.Releasing)
            {
                throw new InvalidOperationException(
                    "Order can only complete refusal after inventory release when it is Refused and releasing inventory.");
            }

            FullfillmentStatus = FullfillmentStatus.Cancelled;
        }

        public void Complete()
        {
            if (Status != OrderStatus.Accepted || FullfillmentStatus != FullfillmentStatus.Dispatched)
            {
                throw new InvalidOperationException(
                    "Order can only be completed when it is Accepted and fulfillment is Dispatched.");
            }

            Status = OrderStatus.Completed;
        }
    }
}
