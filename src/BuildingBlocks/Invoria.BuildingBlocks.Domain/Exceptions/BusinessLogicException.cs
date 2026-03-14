namespace Invoria.BuildingBlocks.Domain.Exceptions;

public class BusinessLogicException : ApplicationExceptionBase
{
    public const string DefaultCode = "business_error";

    public BusinessLogicException(string message)
        : base(DefaultCode, message)
    {
    }

    public BusinessLogicException(string message, Exception? innerException)
        : base(DefaultCode, message, innerException)
    {
    }
}

