namespace Invoria.BuildingBlocks.Domain.Exceptions;

public sealed class BusinessValidationException : ApplicationExceptionBase
{
    public const string DefaultCode = "business_validation_error";
    private const string MultipleErrorsSummary = "One or more business validation errors occurred.";

    public IReadOnlyList<string> Messages { get; }

    public BusinessValidationException(IReadOnlyList<string> messages)
        : base(DefaultCode, BuildSummaryMessage(messages))
    {
        if (messages is null || messages.Count == 0)
        {
            throw new ArgumentException("At least one validation message is required.", nameof(messages));
        }

        if (messages.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Validation messages cannot be null or whitespace.", nameof(messages));
        }

        Messages = messages;
    }

    public BusinessValidationException(params string[] messages)
        : this((IReadOnlyList<string>)messages)
    {
    }

    private static string BuildSummaryMessage(IReadOnlyList<string> messages)
    {
        if (messages is null || messages.Count == 0)
        {
            return MultipleErrorsSummary;
        }

        return messages.Count == 1 ? messages[0] : MultipleErrorsSummary;
    }
}
