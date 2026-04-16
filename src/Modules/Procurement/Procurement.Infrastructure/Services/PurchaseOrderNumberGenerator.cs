using Invoria.Procurement.Application.Services;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Procurement.Infrastructure.Services;

public class PurchaseOrderNumberGenerator(ProcurementDbContext db) : IPurchaseOrderNumberGenerator
{
    private readonly ProcurementDbContext _db = db;

    public async Task<string> GenerateNextAsync(CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                var utcDate = DateTime.UtcNow.Date;
                var sequenceId = $"{utcDate:yyyyMMdd}";
                var sequence = await _db.PurchaseOrderSequences.SingleOrDefaultAsync(x => x.Id == sequenceId, ct);
                if (sequence is null)
                {
                    sequence = PurchaseOrderSequence.Create(utcDate.Year, utcDate.Month, utcDate.Day);
                    _db.PurchaseOrderSequences.Add(sequence);
                }
                var value = sequence.Increment();
                await _db.SaveChangesAsync(ct);
                return $"{utcDate:yyyyMMdd}{value:D5}";
            }
            catch (DbUpdateConcurrencyException) when (attempt < 4)
            {
                _db.ChangeTracker.Clear();
            }
        }

        throw new InvalidOperationException("Failed to generate purchase order number after maximum retries.");
    }
}
