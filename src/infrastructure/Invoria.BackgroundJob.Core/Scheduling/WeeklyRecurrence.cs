namespace Invoria.BackgroundJob.Core.Scheduling;

public sealed record WeeklyRecurrence(
    DayOfWeek Day,
    TimeOnly At) : Recurrence;
