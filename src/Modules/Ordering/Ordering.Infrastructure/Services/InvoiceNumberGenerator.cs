using Invoria.Ordering.Application.Invoices.Services;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Infrastructure.Services;

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly ICounterRepository _counterRepository;
    private readonly Func<DateTime> _utcNowProvider;

    public InvoiceNumberGenerator(ICounterRepository counterRepository, Func<DateTime>? utcNowProvider = null)
    {
        _counterRepository = counterRepository;
        _utcNowProvider = utcNowProvider ?? (() => DateTime.UtcNow);
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = _utcNowProvider();
        var date = DateOnly.FromDateTime(utcNow);
        var sequence = await _counterRepository.IncrementDailyCounterAsync(date, cancellationToken);

        if (sequence > 9999)
        {
            throw new InvalidOperationException(
                $"Daily invoice number limit exceeded for date {utcNow:yyMMdd}. Maximum allowed is 9999.");
        }

        return $"{utcNow:yyMMdd}{sequence:D4}";
    }
}
