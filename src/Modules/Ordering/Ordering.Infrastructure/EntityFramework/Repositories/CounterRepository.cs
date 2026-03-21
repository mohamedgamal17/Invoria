using Invoria.Ordering.Domain.Orders;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Repositories
{
    public class CounterRepository : ICounterRepository
    {
        private readonly OrderingDbContext _dbContext;

        public CounterRepository(OrderingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> IncrementDailyCounterAsync(DateOnly date, CancellationToken cancellationToken = default)
        {

            // Each thread gets its own DbContext
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,cancellationToken);

            var dbset = _dbContext.Set<DailyCounter>();

            var counter = await dbset
                .Where(c => c.Date == date)
                .FirstOrDefaultAsync(cancellationToken);

            int nextValue;

            if (counter == null)
            {
                counter = new DailyCounter
                {
                    Date = date,
                    LastValue = 1
                };
                dbset.Add(counter);
                nextValue = 1;
            }
            else
            {
                counter.LastValue += 1;
                nextValue = counter.LastValue;
                dbset.Update(counter);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return nextValue;
        }
    }
    
}
