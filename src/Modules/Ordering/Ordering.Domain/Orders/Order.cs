using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders.Events;

namespace Invoria.Ordering.Domain.Orders
{
    public class Order : AuditedAggregateRoot
    {
        public string OrderNumber { get;private set; }
        public string CustomerId { get; private set; }
        public List<OrderItem> Items { get; private set; } 
        public List<OrderFailureDetails> FailureDetails { get; private set; }
        public List<OrderStateTransitionHistory> StateTransitionHistory { get; private set; }
        public List<OrderPayment> Payments { get; private set; }
        public IReadOnlyList<OrderReturnItem> ReturnItems => _returnItems;
        public OrderPaymentType PaymentType { get; private set; }
        public OrderStatus Status { get; private set; }
        public FullfillmentStatus FullfillmentStatus { get; set; }

        private readonly List<OrderReturnItem> _returnItems = new();

        public decimal TotalOrderAmount => Items.Sum(i => i.Price * i.Quantity);

        public decimal NetOfTotalOrderAmount => Items.Sum(i =>
            i.Price * Math.Max(0, i.Quantity - ReturnedQuantity(i.Id)));

        public decimal AmountPaid { get; private set; }

        public decimal AmountOutstanding { get; private set; }

        public OrderPaymentStatus PaymentStatus { get; private set; }

        private Order()
        {
            Items = new List<OrderItem>();
            FailureDetails = new List<OrderFailureDetails>();
            StateTransitionHistory = new List<OrderStateTransitionHistory>();
            Payments = new List<OrderPayment>();
            PaymentStatus = OrderPaymentStatus.Unpaid;
        }

        public Order(string orderNumber, string customerId, OrderPaymentType paymentType = OrderPaymentType.Immediate)
        {
            OrderNumber = orderNumber;
            CustomerId = customerId;
            PaymentType = paymentType;
            Items = new List<OrderItem>();
            FailureDetails = new List<OrderFailureDetails>();
            StateTransitionHistory = new List<OrderStateTransitionHistory>();
            Payments = new List<OrderPayment>();
            Status = OrderStatus.Pending;
            FullfillmentStatus = FullfillmentStatus.Pending;
            AmountPaid = 0m;
            AmountOutstanding = 0m;
            PaymentStatus = OrderPaymentStatus.Unpaid;
        }

        public void RecordPayment(decimal paidAmount, OrderPaymentMethod method, DateTimeOffset paidAt)
        {
            if (Status != OrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Payments can only be recorded after the order is completed.");
            }

            if (TotalOrderAmount <= 0m)
            {
                throw new InvalidOperationException(
                    "Cannot record payments when the order has no positive total amount.");
            }

            if (paidAmount <= 0m)
            {
                throw new InvalidOperationException("Payment amount must be greater than zero.");
            }

            var paidSoFar = Payments.Sum(p => p.PaidAmount);
            var outstandingBefore = Math.Max(0m, TotalOrderAmount - paidSoFar);

            if (outstandingBefore <= 0m)
            {
                throw new InvalidOperationException("Order is already fully paid.");
            }

            if (PaymentType == OrderPaymentType.Immediate)
            {
                if (Payments.Count > 0)
                {
                    throw new InvalidOperationException(
                        "Immediate payment orders accept only a single full payment.");
                }

                if (paidAmount != TotalOrderAmount)
                {
                    throw new InvalidOperationException(
                        "Immediate payment orders require a single payment equal to the total order amount.");
                }
            }
            else
            {
                if (paidAmount > outstandingBefore)
                {
                    throw new InvalidOperationException(
                        "Payment amount cannot exceed the outstanding balance.");
                }
            }

            Payments.Add(new OrderPayment(Id, paidAmount, method, paidAt));
            RefreshPaymentSummary();
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
            RefreshPaymentSummary();
        }

        private void RefreshPaymentSummary()
        {
            AmountPaid = Payments.Sum(p => p.PaidAmount);
            AmountOutstanding = Math.Max(0m, TotalOrderAmount - AmountPaid);

            if (TotalOrderAmount == 0m)
            {
                PaymentStatus = OrderPaymentStatus.Unpaid;
                return;
            }

            if (AmountPaid == 0m)
            {
                PaymentStatus = OrderPaymentStatus.Unpaid;
                return;
            }

            if (AmountPaid >= TotalOrderAmount)
            {
                PaymentStatus = OrderPaymentStatus.Paid;
                return;
            }

            PaymentStatus = OrderPaymentStatus.Partial;
        }

        public void ReplaceFailureDetails(List<OrderFailureDetails> failureDetails)
        {
            Guard.Against.Null(failureDetails);
            FailureDetails = failureDetails;
        }


        /// <summary>
        /// Accepts the order for fulfillment allocation when fulfillment is <see cref="FullfillmentStatus.Pending"/>
        /// or <see cref="FullfillmentStatus.OnHold"/> (for example after <see cref="Reopen"/>).
        /// </summary>
        public void Accept()
        {
            var fromStatus = Status;
            var fromFullfillmentStatus = FullfillmentStatus;

            if (Status != OrderStatus.Pending && Status != OrderStatus.Reopened)
            {
                throw new InvalidOperationException(
                    "Order can only be accepted when it is Pending or Reopened.");
            }

            if (FullfillmentStatus != FullfillmentStatus.Pending
                && FullfillmentStatus != FullfillmentStatus.OnHold)
            {
                throw new InvalidOperationException(
                    "Order can only be accepted when fulfillment is Pending or On Hold.");
            }

            Status = OrderStatus.Accepted;
            FullfillmentStatus = FullfillmentStatus.Allocating;
            AppendStateTransitionHistory(fromStatus, fromFullfillmentStatus, reason: null);
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
                    var pendingFromStatus = Status;
                    var pendingFromFullfillmentStatus = FullfillmentStatus;
                    FullfillmentStatus = FullfillmentStatus.OnHold;
                    Status = OrderStatus.Reopened;
                    AppendStateTransitionHistory(pendingFromStatus, pendingFromFullfillmentStatus, reason: null);
                    return;
                case FullfillmentStatus.Allocating:
                case FullfillmentStatus.Allocated:
                    var allocatedFromStatus = Status;
                    var allocatedFromFullfillmentStatus = FullfillmentStatus;
                    FullfillmentStatus = FullfillmentStatus.Releasing;
                    AppendStateTransitionHistory(allocatedFromStatus, allocatedFromFullfillmentStatus, reason: null);
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
            var fromStatus = Status;
            var fromFullfillmentStatus = FullfillmentStatus;

            if (Status != OrderStatus.Accepted || FullfillmentStatus != FullfillmentStatus.Releasing)
            {
                throw new InvalidOperationException(
                    "Order can only complete reopen after inventory release when it is Accepted and releasing inventory.");
            }

            FullfillmentStatus = FullfillmentStatus.OnHold;
            Status = OrderStatus.Reopened;
            AppendStateTransitionHistory(fromStatus, fromFullfillmentStatus, reason: null);
        }

        public void Cancel()
        {
            var fromStatus = Status;
            var fromFullfillmentStatus = FullfillmentStatus;

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
            AppendStateTransitionHistory(fromStatus, fromFullfillmentStatus, reason: null);
        }

        /// <summary>
        /// Cancels the order when inventory allocation fails after the order was accepted.
        /// </summary>
        public void CancelDueToAllocationFailure(string reason)
        {
            Guard.Against.NullOrWhiteSpace(reason);
            var fromStatus = Status;
            var fromFullfillmentStatus = FullfillmentStatus;

            if (Status != OrderStatus.Accepted || FullfillmentStatus != FullfillmentStatus.Allocating)
            {
                throw new InvalidOperationException(
                    "Order can only be cancelled due to allocation failure when it is Accepted and allocating inventory.");
            }

            Status = OrderStatus.Cancelled;
            FullfillmentStatus = FullfillmentStatus.Pending;
            AppendStateTransitionHistory(fromStatus, fromFullfillmentStatus, reason);
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

            var fromStatus = Status;
            var fromFullfillmentStatus = FullfillmentStatus;

            if (Status != OrderStatus.Accepted || FullfillmentStatus != FullfillmentStatus.Allocating)
            {
                throw new InvalidOperationException(
                    "Order can only be marked inventory allocated when it is Accepted and allocating inventory.");
            }

            FullfillmentStatus = FullfillmentStatus.Allocated;
            AppendStateTransitionHistory(fromStatus, fromFullfillmentStatus, reason: null);
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

            var fromStatus = Status;
            var fromFullfillmentStatus = FullfillmentStatus;

            if (Status != OrderStatus.Accepted || FullfillmentStatus != FullfillmentStatus.Allocated)
            {
                throw new InvalidOperationException(
                    "Order can only be marked dispatched when it is Accepted and inventory is allocated.");
            }

            FullfillmentStatus = FullfillmentStatus.Dispatched;
            AppendStateTransitionHistory(fromStatus, fromFullfillmentStatus, reason: null);

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
                var completedFromStatus = Status;
                var completedFromFullfillmentStatus = FullfillmentStatus;
                Status = OrderStatus.Refused;
                AppendStateTransitionHistory(completedFromStatus, completedFromFullfillmentStatus, reason: null);
                AddDomainEvent(new OrderRefusedDomainEvent(Id, OrderNumber, CustomerId));
                return;
            }

            switch (FullfillmentStatus)
            {
                case FullfillmentStatus.Allocated:
                    var allocatedFromStatus = Status;
                    var allocatedFromFullfillmentStatus = FullfillmentStatus;
                    Status = OrderStatus.Refused;
                    FullfillmentStatus = FullfillmentStatus.Releasing;
                    AppendStateTransitionHistory(allocatedFromStatus, allocatedFromFullfillmentStatus, reason: null);
                    var refusalLines = Items
                        .Select(i => new OrderDispatchedLine(i.Id, i.ProductId, i.Quantity))
                        .ToList();
                    AddDomainEvent(new OrderRefusalReleaseRequestedDomainEvent(Id, OrderNumber, CustomerId, refusalLines));
                    return;
                case FullfillmentStatus.Releasing:
                    throw new InvalidOperationException(
                        "Order cannot be refused while inventory allocations are being released; complete or cancel the reopen flow first.");
                case FullfillmentStatus.Dispatched:
                    var dispatchedFromStatus = Status;
                    var dispatchedFromFullfillmentStatus = FullfillmentStatus;
                    Status = OrderStatus.Refused;
                    AppendStateTransitionHistory(dispatchedFromStatus, dispatchedFromFullfillmentStatus, reason: null);
                    AddDomainEvent(new OrderRefusedDomainEvent(Id, OrderNumber, CustomerId));
                    return;
                case FullfillmentStatus.Allocating:
                case FullfillmentStatus.Pending:
                case FullfillmentStatus.OnHold:
                    var nonAllocatedFromStatus = Status;
                    var nonAllocatedFromFullfillmentStatus = FullfillmentStatus;
                    Status = OrderStatus.Refused;
                    FullfillmentStatus = FullfillmentStatus.Cancelled;
                    AppendStateTransitionHistory(nonAllocatedFromStatus, nonAllocatedFromFullfillmentStatus, reason: null);
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
            var fromStatus = Status;
            var fromFullfillmentStatus = FullfillmentStatus;

            if (Status != OrderStatus.Refused || FullfillmentStatus != FullfillmentStatus.Releasing)
            {
                throw new InvalidOperationException(
                    "Order can only complete refusal after inventory release when it is Refused and releasing inventory.");
            }

            FullfillmentStatus = FullfillmentStatus.Cancelled;
            AppendStateTransitionHistory(fromStatus, fromFullfillmentStatus, reason: null);
        }

        /// <summary>
        /// Marks the order as shipped after it has been dispatched to the customer.
        /// Idempotent when <see cref="OrderStatus"/> is already <see cref="OrderStatus.Shipped"/>.
        /// </summary>
        public void MarkShipped()
        {
            if (Status == OrderStatus.Shipped && FullfillmentStatus == FullfillmentStatus.Dispatched)
            {
                return;
            }

            var fromStatus = Status;
            var fromFullfillmentStatus = FullfillmentStatus;

            if (Status != OrderStatus.Accepted || FullfillmentStatus != FullfillmentStatus.Dispatched)
            {
                throw new InvalidOperationException(
                    "Order can only be marked shipped when it is Accepted and fulfillment is Dispatched.");
            }

            Status = OrderStatus.Shipped;
            AppendStateTransitionHistory(fromStatus, fromFullfillmentStatus, reason: null);
        }

        /// <summary>
        /// Replaces the full customer return list for a shipped order in one atomic batch.
        /// Duplicate lines in the request are grouped by order line id and quantities are summed.
        /// An empty list clears all returns. When all lines are fully returned, <see cref="Complete"/> cancels the order.
        /// </summary>
        public Result RecordReturnItems(IReadOnlyList<OrderReturnItem> returnItems)
        {
            Guard.Against.Null(returnItems);

            var errors = ValidateReturnItems(returnItems);
            if (errors.Count > 0)
            {
                return Result.Failure(new BusinessValidationException(errors));
            }

            var normalizedItems = NormalizeReturnItems(returnItems);
            _returnItems.Clear();
            foreach (var returnItem in normalizedItems)
            {
                _returnItems.Add(returnItem);
            }

            return Result.Success();
        }

        private List<string> ValidateReturnItems(IReadOnlyList<OrderReturnItem> returnItems)
        {
            var errors = new List<string>();

            if (Status != OrderStatus.Shipped)
            {
                errors.Add("Return items can only be recorded when the order is Shipped.");
            }

            var quantitiesByLine = returnItems
                .GroupBy(r => r.OrderItemId)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

            foreach (var (orderItemId, batchQuantity) in quantitiesByLine)
            {
                var line = Items.SingleOrDefault(i => i.Id == orderItemId);
                if (line is null)
                {
                    errors.Add($"Return item references unknown order line '{orderItemId}'.");
                    continue;
                }

                if (batchQuantity > line.Quantity)
                {
                    errors.Add(
                        $"Return quantity for order line '{orderItemId}' cannot exceed ordered quantity.");
                }
            }

            return errors;
        }

        private static List<OrderReturnItem> NormalizeReturnItems(IReadOnlyList<OrderReturnItem> returnItems)
        {
            return returnItems
                .GroupBy(r => r.OrderItemId)
                .Select(g => new OrderReturnItem(g.Key, g.Sum(r => r.Quantity)))
                .ToList();
        }

        public void Complete()
        {
            var fromStatus = Status;
            var fromFullfillmentStatus = FullfillmentStatus;

            if (Status != OrderStatus.Shipped || FullfillmentStatus != FullfillmentStatus.Dispatched)
            {
                throw new InvalidOperationException(
                    "Order can only be completed when it is Shipped and fulfillment is Dispatched.");
            }

            if (AllItemsFullyReturned())
            {
                Status = OrderStatus.Cancelled;
                AppendStateTransitionHistory(fromStatus, fromFullfillmentStatus, reason: "All order items returned");
                return;
            }

            Status = OrderStatus.Completed;
            AppendStateTransitionHistory(fromStatus, fromFullfillmentStatus, reason: null);
        }

        private int ReturnedQuantity(string orderItemId)
        {
            return _returnItems
                .Where(r => r.OrderItemId == orderItemId)
                .Sum(r => r.Quantity);
        }

        private bool AllItemsFullyReturned()
        {
            return Items.Count > 0
                && Items.All(i => ReturnedQuantity(i.Id) >= i.Quantity);
        }

        private void AppendStateTransitionHistory(
            OrderStatus fromStatus,
            FullfillmentStatus fromFullfillmentStatus,
            string? reason)
        {
            var orderId = string.IsNullOrWhiteSpace(Id)
                ? Guid.NewGuid().ToString("N")
                : Id;

            StateTransitionHistory.Add(new OrderStateTransitionHistory(
                orderId,
                fromStatus,
                Status,
                fromFullfillmentStatus,
                FullfillmentStatus,
                DateTimeOffset.UtcNow,
                reason));
        }
    }
}
