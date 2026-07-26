namespace Invoria.BackgroundJob.Core.Scheduling;

public sealed record DailyRecurrence(
    TimeOnly At) : Recurrence;
