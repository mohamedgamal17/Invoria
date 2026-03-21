namespace Invoria.Ordering.Application.Orders.Services
{
    public interface IOrderNumberGenerator
    {
        Task<string> GenerateAsync(CancellationToken cancellationToken = default);
    }
}
