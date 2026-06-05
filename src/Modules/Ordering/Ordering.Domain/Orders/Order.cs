using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Orders.Enums;
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

        public string? AllocationId { get; private set; }

        private Order()
        {
            Items = new List<OrderItem>();
            Payments = new List<OrderPayment>();
            PaymentStatus = OrderPaymentStatus.Unpaid;
        }

        private Order(
            string id,
            string orderNumber,
            string customerId,
            OrderPaymentType paymentType)
        {
            Id = id;
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

        public static Order Create(
            string orderNumber,
            string customerId,
            OrderPaymentType paymentType,
            List<OrderItem> items)
        {
            var order = new Order(
                Guid.NewGuid().ToString("N"),
                orderNumber,
                customerId,
                paymentType);

            order.UpdateItems(items);
            order.AddDomainEvent(new OrderCreatedDomainEvent(order));

            return order;
        }

        public void RecordPayment(decimal paidAmount, OrderPaymentMethod method, DateTimeOffset paidAt)
        {
            if (Status != OrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Payments can only be recorded after the order is completed.");
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

        public void Accept()
        {
            if (Status != OrderStatus.Pending && Status != OrderStatus.Revision)
            {
                throw new InvalidOperationException(
                    "Order can only be accepted when it is Pending or Revision.");
            }

            Status = OrderStatus.Processing;
            AddDomainEvent(new OrderAcceptedDomainEvent(this));
        }

        public void RecordAllocation(string allocationId)
        {
            Guard.Against.NullOrWhiteSpace(allocationId);
            Guard.Against.OutOfRange(
                allocationId.Length,
                nameof(allocationId),
                1,
                OrderTableConsts.AllocationIdMaxLength);

            if (Status != OrderStatus.Processing)
            {
                throw new InvalidOperationException(
                    "Order allocation can only be recorded when the order is Processing.");
            }

            AllocationId = allocationId;
        }

        public void Revise()
        {
            if (Status != OrderStatus.Pending && Status != OrderStatus.Revision)
            {
                throw new InvalidOperationException(
                    "Order can only be revised when it is Pending or Revision.");
            }

            Status = OrderStatus.Processing;
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

        public Result RecordReturnItems(IReadOnlyList<OrderReturnItem> returnItems)
        {
            Guard.Against.Null(returnItems);

            var normalizedItems = NormalizeReturnItems(returnItems);
            ReturnItems.Clear();
            foreach (var returnItem in normalizedItems)
            {
                ReturnItems.Add(returnItem);
            }

            RefreshPaymentSummary();

            return Result.Success();
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
