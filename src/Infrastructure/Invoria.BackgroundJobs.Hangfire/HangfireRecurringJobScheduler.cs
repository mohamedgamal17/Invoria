using Hangfire;
using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJob.Core.Scheduling;
using Invoria.BackgroundJobs.Hangfire.Execution;

namespace Invoria.BackgroundJobs.Hangfire;

public sealed class HangfireRecurringJobScheduler : IRecurringJobScheduler
{
    private readonly IRecurringJobManager _manager;

    public HangfireRecurringJobScheduler(IRecurringJobManager manager)
    {
        _manager = manager;
    }

    public void AddOrUpdate<TJob>(string recurringJobId, Recurrence recurrence)
        where TJob : class, IJob
    {
        string cronExpression = MapToCronExpression(recurrence);

        string jobTypeName = typeof(TJob).AssemblyQualifiedName
            ?? throw new InvalidOperationException(
                $"Could not resolve AssemblyQualifiedName for {typeof(TJob).FullName}.");

        _manager.AddOrUpdate<HangfireJobDispatcher>(
            recurringJobId,
            dispatcher => dispatcher.ExecuteAsync(jobTypeName, CancellationToken.None),
            cronExpression);
    }

    public void Remove(string recurringJobId)
    {
        _manager.RemoveIfExists(recurringJobId);
    }

    public void Trigger(string recurringJobId)
    {
        _manager.Trigger(recurringJobId);
    }

    private static string MapToCronExpression(Recurrence recurrence)
    {
        return recurrence switch
        {
            CustomRecurrence custom => custom.Expression,
            DailyRecurrence daily => Cron.Daily(daily.At.Hour, daily.At.Minute),
            WeeklyRecurrence weekly => Cron.Weekly(weekly.Day, weekly.At.Hour, weekly.At.Minute),
            MonthlyRecurrence monthly => Cron.Monthly(monthly.Day, monthly.At.Hour, monthly.At.Minute),
            IntervalRecurrence interval => MapIntervalRecurrence(interval),
            _ => throw new ArgumentOutOfRangeException(nameof(recurrence), recurrence.GetType().Name, null)
        };
    }

    private static string MapIntervalRecurrence(IntervalRecurrence interval)
    {
        double totalMinutes = interval.Interval.TotalMinutes;

        if (totalMinutes < 60)
        {
            string expression = Cron.MinuteInterval((int)totalMinutes);
            return expression;
        }

        double totalHours = interval.Interval.TotalHours;

        if (totalHours < 24)
        {
            string expression = Cron.HourInterval((int)totalHours);
            return expression;
        }

        string dayExpression = Cron.DayInterval((int)interval.Interval.TotalDays);
        return dayExpression;
    }
}
