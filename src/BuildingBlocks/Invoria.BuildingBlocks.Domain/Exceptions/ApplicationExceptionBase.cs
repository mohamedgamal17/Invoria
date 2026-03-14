namespace Invoria.BuildingBlocks.Domain.Exceptions;

public abstract class ApplicationExceptionBase : Exception
{
    public string Code { get; }

    protected ApplicationExceptionBase(string code, string message)
        : base(message)
    {
        Code = code;
    }

    protected ApplicationExceptionBase(string code, string message, Exception? innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}

