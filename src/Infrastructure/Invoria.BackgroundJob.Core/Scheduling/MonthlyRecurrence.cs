namespace Invoria.BackgroundJob.Core.Scheduling;

public sealed record MonthlyRecurrence(
    int Day,
    TimeOnly At) : Recurrence;
