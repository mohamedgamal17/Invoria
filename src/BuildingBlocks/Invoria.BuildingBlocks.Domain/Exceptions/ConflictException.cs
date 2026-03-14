namespace Invoria.BuildingBlocks.Domain.Exceptions;

public class ConflictException : ApplicationExceptionBase
{
    public const string DefaultCode = "conflict";

    public ConflictException(string message)
        : base(DefaultCode, message)
    {
    }

    public ConflictException(string message, Exception? innerException)
        : base(DefaultCode, message, innerException)
    {
    }
}

