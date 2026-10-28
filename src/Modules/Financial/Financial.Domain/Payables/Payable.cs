using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Financial.Contracts.Receivables.Enums;

namespace Invoria.Financial.Domain.Payables
{
    public sealed class Payable : AuditedAggregateRoot
    {
        private readonly List<PayableSettlement> _settlements = new();

        public string PartyId { get; private set; } = default!;
        public string SourceId { get; private set; } = default!;
        public decimal Amount { get; private set; }
        public decimal OutstandingAmount { get; private set; }
        public FinancialObligationStatus Status { get; private set; }

        public IReadOnlyCollection<PayableSettlement> Settlements => _settlements.AsReadOnly();

        public decimal TotalSettledAmount => Amount - OutstandingAmount;

        private Payable()
        {
            OutstandingAmount = 0m;
        }

        private Payable(string partyId, string sourceId, decimal amount)
        {
            PartyId = partyId;
            SourceId = sourceId;
            Amount = amount;
            OutstandingAmount = amount;
            Status = FinancialObligationStatus.Outstanding;
        }

        public static Payable Create(string partyId, string sourceId, decimal amount)
        {
            Guard.Against.NullOrWhiteSpace(partyId);
            Guard.Against.NullOrWhiteSpace(sourceId);
            Guard.Against.NegativeOrZero(amount);

            return new Payable(partyId, sourceId, amount);
        }

        public void Settle(decimal amount, DateTimeOffset? settledAt = null)
        {
            Guard.Against.NegativeOrZero(amount);

            var outstandingBefore = OutstandingAmount;

            if (Status == FinancialObligationStatus.Paid)
            {
                throw new InvalidOperationException(
                    $"Payable {Id} is already fully settled.");
            }

            if (amount > outstandingBefore)
            {
                throw new InvalidOperationException(
                    $"Settlement amount {amount} exceeds the outstanding balance {outstandingBefore} on payable {Id}.");
            }

            var settlement = new PayableSettlement(
                Id,
                amount,
                settledAt ?? DateTimeOffset.UtcNow);

            _settlements.Add(settlement);
            RefreshSettlementSummary();
            RefreshStatus();
        }

        private void RefreshSettlementSummary()
        {
            OutstandingAmount = Math.Max(0m, Amount - _settlements.Sum(s => s.Amount));
        }

        private void RefreshStatus()
        {
            if (OutstandingAmount <= 0m)
            {
                Status = FinancialObligationStatus.Paid;
                return;
            }

            if (TotalSettledAmount > 0m)
            {
                Status = FinancialObligationStatus.PartiallyPaid;
                return;
            }

            Status = FinancialObligationStatus.Outstanding;
        }
    }
}
