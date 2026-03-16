using Invoria.BuildingBlocks.Infrastructure.Common;

namespace Invoria.BuildingBlocks.Infrastructure.Common;

public class Envelope
{
    public bool IsSuccess { get; set; }
    public ApiProblemDetails? Error { get; set; }

    public Envelope()
    {
        
    }
    public Envelope(bool isSuccess, ApiProblemDetails? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Envelope Success() => new Envelope(true, null);
    public static Envelope Failure(ApiProblemDetails error) => new Envelope(false, error);
}

public class Envelope<T> : Envelope
{
    public T? Result { get; set; }

    public Envelope()
    {
        
    }
    public Envelope(bool isSuccess, T? result, ApiProblemDetails? error)
        : base(isSuccess, error)
    {
        Result = result;
    }

    public static Envelope<T> Success(T result) => new Envelope<T>(true, result, null);
    public static new Envelope<T> Failure(ApiProblemDetails error) => new Envelope<T>(false, default, error);
}
