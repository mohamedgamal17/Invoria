namespace Invoria.BackgroundJob.Core.Scheduling;

public sealed record IntervalRecurrence(
    TimeSpan Interval) : Recurrence;
