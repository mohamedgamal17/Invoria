using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Financial.Domain.Receivables
{
    public class ReceivableSettlement : AuditedEntity
    {
        public string ReceivableId { get; private set; } = null!;
        public decimal Amount { get; private set; }
        public DateTimeOffset SettledAt { get; private set; }
        public Receivable? Receivable { get; private set; }

        private ReceivableSettlement()
        {
        }

        public ReceivableSettlement(string receivableId, decimal amount, DateTimeOffset settledAt)
        {
            Guard.Against.NullOrWhiteSpace(receivableId);
            Guard.Against.NegativeOrZero(amount);

            ReceivableId = receivableId;
            Amount = amount;
            SettledAt = settledAt;
        }
    }
}
