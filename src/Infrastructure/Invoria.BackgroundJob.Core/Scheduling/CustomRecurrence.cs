namespace Invoria.BackgroundJob.Core.Scheduling;

public sealed record CustomRecurrence(
    string Expression) : Recurrence;
