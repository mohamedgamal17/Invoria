namespace Invoria.Ordering.Application.Invoices.Services;

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
