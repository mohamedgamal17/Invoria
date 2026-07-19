namespace Invoria.BackgroundJob.Core;

public readonly record struct JobId
{
    public string Value { get; }

    public JobId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
    }

    public override string ToString()
        => Value;

    public static implicit operator string(JobId jobId)
        => jobId.Value;

    public static explicit operator JobId(string value)
        => new(value);
}
