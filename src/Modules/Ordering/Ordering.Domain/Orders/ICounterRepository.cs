namespace Invoria.Ordering.Domain.Orders
{
    public interface ICounterRepository
    {
        Task<int> IncrementDailyCounterAsync(DateOnly date, CancellationToken cancellationToken = default);
    }
}
