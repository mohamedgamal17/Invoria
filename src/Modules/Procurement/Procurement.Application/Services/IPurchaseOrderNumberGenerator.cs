namespace Invoria.Procurement.Application.Services;

public interface IPurchaseOrderNumberGenerator
{
    Task<string> GenerateNextAsync(CancellationToken ct = default);
}
