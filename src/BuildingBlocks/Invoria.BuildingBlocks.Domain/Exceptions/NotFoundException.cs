namespace Invoria.BuildingBlocks.Domain.Exceptions;

public class NotFoundException : ApplicationExceptionBase
{
    public const string DefaultCode = "not_found";

    public NotFoundException(string message)
        : base(DefaultCode, message)
    {
    }

    public NotFoundException(string message, Exception? innerException)
        : base(DefaultCode, message, innerException)
    {
    }
}

