using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Financial.Domain.Payables
{
    public class PayableSettlement : AuditedEntity
    {
        public string PayableId { get; private set; } = null!;
        public decimal Amount { get; private set; }
        public DateTimeOffset SettledAt { get; private set; }
        public Payable? Payable { get; private set; }

        private PayableSettlement()
        {
        }

        public PayableSettlement(string payableId, decimal amount, DateTimeOffset settledAt)
        {
            Guard.Against.NullOrWhiteSpace(payableId);
            Guard.Against.NegativeOrZero(amount);

            PayableId = payableId;
            Amount = amount;
            SettledAt = settledAt;
        }
    }
}
