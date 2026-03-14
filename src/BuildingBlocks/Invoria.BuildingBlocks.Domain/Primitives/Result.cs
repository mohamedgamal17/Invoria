namespace Invoria.BuildingBlocks.Domain.Primitives;


public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Exception? Exception { get; }

    public T? Value { get; }

    internal Result(bool isSuccess, T? value, Exception? exception)
    {
        if (isSuccess && exception is not null)
            throw new ArgumentException("A successful result cannot have an exception.", nameof(exception));

        if (!isSuccess && exception is null)
            throw new ArgumentNullException(nameof(exception), "A failed result must have a non-null exception.");

        IsSuccess = isSuccess;
        Value = value;
        Exception = exception;
    }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(true, value, null);
    }

    public static implicit operator Result<T>(Exception exception)
    {
        if (exception is null)
            throw new ArgumentNullException(nameof(exception));

        return new Result<T>(false, default, exception);
    }
}

public class Result : Result<Empty>
{
    public new bool IsSuccess => base.IsSuccess;
    public new bool IsFailure => base.IsFailure;
    public new Exception? Exception => base.Exception;

    protected Result(bool isSuccess, Exception? exception)
        : base(isSuccess, Empty.Value, exception)
    {
    }

    public static Result Success() => new(true, null);

    public static Result Failure(Exception exception)
    {
        if (exception is null)
            throw new ArgumentNullException(nameof(exception));

        return new Result(false, exception);
    }

    public static Result<T> Success<T>(T value) => new(true, value, null);

    public static Result<T> Failure<T>(Exception exception)
    {
        if (exception is null)
            throw new ArgumentNullException(nameof(exception));

        return new Result<T>(false, default, exception);
    }
}