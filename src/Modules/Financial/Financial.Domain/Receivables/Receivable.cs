using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Financial.Contracts.Receivables.Enums;

namespace Invoria.Financial.Domain.Receivables
{
    public sealed class Receivable : AuditedAggregateRoot
    {
        public string PartyId { get; private set; } = default!;
        public string SourceId { get; private set; } = default!;
        public decimal Amount { get; private set; }
        public FinancialObligationStatus Status { get; private set; }

        private Receivable()
        {
        }

        private Receivable(string partyId, string sourceId, decimal amount)
        {
            PartyId = partyId;
            SourceId = sourceId;
            Amount = amount;
            Status = FinancialObligationStatus.Outstanding;
        }

        public static Receivable Create(string partyId, string sourceId, decimal amount)
        {
            Guard.Against.NullOrWhiteSpace(partyId);
            Guard.Against.NullOrWhiteSpace(sourceId);
            Guard.Against.NegativeOrZero(amount);

            return new Receivable(partyId, sourceId, amount);
        }
    }
}
