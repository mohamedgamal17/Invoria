namespace Invoria.Ordering.Infrastructure.EntityFramework
{
    public class DailyCounter
    {
        public DateOnly Date { get; set; }

        public int LastValue { get; set; }
    }
}
