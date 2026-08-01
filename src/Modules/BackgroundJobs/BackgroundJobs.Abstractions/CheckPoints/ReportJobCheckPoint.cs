namespace Invoria.BackgroundJobs.Abstractions.CheckPoints;

public class ReportJobCheckPoint
{
    public string LastId { get; set; } = string.Empty;

    public long LastIndex { get; set; }
}
