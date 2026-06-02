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
        public List<OrderPayment> Payments { get; private set; }
        public List<OrderReturnItem> ReturnItems { get; private set; } = new();
        public OrderPaymentType PaymentType { get; private set; }
        public OrderStatus Status { get; private set; }

        public decimal TotalOrderAmount => Items.Sum(i => i.Price * i.Quantity);

        public decimal NetOfTotalOrderAmount => Items.Sum(i =>
            i.Price * Math.Max(0, i.Quantity - ReturnedQuantity(i.Id)));

        public decimal AmountPaid { get; private set; }

        public decimal AmountOutstanding { get; private set; }

        public OrderPaymentStatus PaymentStatus { get; private set; }

        private Order()
        {
            Items = new List<OrderItem>();
            Payments = new List<OrderPayment>();
            PaymentStatus = OrderPaymentStatus.Unpaid;
        }

        public Order(string orderNumber, string customerId, OrderPaymentType paymentType = OrderPaymentType.Immediate)
        {
            OrderNumber = orderNumber;
            CustomerId = customerId;
            PaymentType = paymentType;
            Items = new List<OrderItem>();
            Payments = new List<OrderPayment>();
            Status = OrderStatus.Pending;
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

            if (NetOfTotalOrderAmount <= 0m)
            {
                throw new InvalidOperationException(
                    "Cannot record payments when the order has no positive total amount.");
            }

            if (paidAmount <= 0m)
            {
                throw new InvalidOperationException("Payment amount must be greater than zero.");
            }

            var paidSoFar = Payments.Sum(p => p.PaidAmount);
            var outstandingBefore = Math.Max(0m, NetOfTotalOrderAmount - paidSoFar);

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

                if (paidAmount != NetOfTotalOrderAmount)
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

            if (Status != OrderStatus.Pending && Status != OrderStatus.Revision)
            {
                throw new InvalidOperationException(
                    "Order items can only be updated when the order is Pending or Revision.");
            }

            Items = items;
            RefreshPaymentSummary();
        }

        private void RefreshPaymentSummary()
        {
            AmountPaid = Payments.Sum(p => p.PaidAmount);
            AmountOutstanding = Math.Max(0m, NetOfTotalOrderAmount - AmountPaid);

            if (NetOfTotalOrderAmount == 0m)
            {
                PaymentStatus = OrderPaymentStatus.Unpaid;
                return;
            }

            if (AmountPaid == 0m)
            {
                PaymentStatus = OrderPaymentStatus.Unpaid;
                return;
            }

            if (AmountPaid >= NetOfTotalOrderAmount)
            {
                PaymentStatus = OrderPaymentStatus.Paid;
                return;
            }

            PaymentStatus = OrderPaymentStatus.Partial;
        }

        /// <summary>
        /// Accepts the order for fulfillment allocation when it is <see cref="OrderStatus.Pending"/> or
        /// <see cref="OrderStatus.Revision"/>.
        /// </summary>
        public void Accept()
        {
            if (Status != OrderStatus.Pending && Status != OrderStatus.Revision)
            {
                throw new InvalidOperationException(
                    "Order can only be accepted when it is Pending or Revision.");
            }

            Status = OrderStatus.Processing;
            var lines = Items
                .Select(i => new OrderAcceptedLine(i.Id, i.ProductId, i.Quantity))
                .ToList();
        }

        /// <summary>
        /// Moves a processing order into revision so it can be edited and accepted again.
        /// </summary>
        public void Revise()
        {
            if (Status != OrderStatus.Processing)
            {
                throw new InvalidOperationException(
                    "Order can only be revised when it is Processing.");
            }

            Status = OrderStatus.Revision;
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Order can only be cancelled when the order is not Completed.");
            }

            Status = OrderStatus.Cancelled;
        }

        /// <summary>
        /// Refuses the order when it is <see cref="OrderStatus.Processing"/> or <see cref="OrderStatus.Completed"/>.
        /// </summary>
        public void Refuse()
        {
            if (Status != OrderStatus.Processing && Status != OrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Order can only be refused when it is Processing or Completed.");
            }

            if (Status == OrderStatus.Processing)
            {
                var refusalLines = Items
                    .Select(i => new OrderDispatchedLine(i.Id, i.ProductId, i.Quantity))
                    .ToList();
                AddDomainEvent(new OrderRefusalReleaseRequestedDomainEvent(Id, OrderNumber, CustomerId, refusalLines));
            }

            Status = OrderStatus.Cancelled;
            AddDomainEvent(new OrderRefusedDomainEvent(Id, OrderNumber, CustomerId));
        }


        /// <summary>
        /// Replaces the full customer return list for a shipped order in one atomic batch.
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
            ReturnItems.Clear();
            foreach (var returnItem in normalizedItems)
            {
                ReturnItems.Add(returnItem);
            }

            RefreshPaymentSummary();

            return Result.Success();
        }

        private List<string> ValidateReturnItems(IReadOnlyList<OrderReturnItem> returnItems)
        {
            var errors = new List<string>();

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
            if (Status != OrderStatus.Processing)
            {
                throw new InvalidOperationException(
                    "Order can only be completed when it is Processing.");
            }

            if (AllItemsFullyReturned())
            {
                Status = OrderStatus.Cancelled;
                return;
            }

            Status = OrderStatus.Completed;
        }

        private int ReturnedQuantity(string orderItemId)
        {
            return ReturnItems
                .Where(r => r.OrderItemId == orderItemId)
                .Sum(r => r.Quantity);
        }

        private bool AllItemsFullyReturned()
        {
            return Items.Count > 0
                && Items.All(i => ReturnedQuantity(i.Id) >= i.Quantity);
        }
    }
}
