using System.Security.AccessControl;

namespace Invoria.BuildingBlocks.Domain.Primitives;



public class Result<T> 
{
    public T? Value { get; }

    internal Result(bool isSuccess, T? value, Exception? exception)
        
    {
        Value = value;
    }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(true, value , null);
    }

    public static implicit operator Result<T>(Exception exception)
    {
        return new Result<T>(false, default, exception);
    }
}

public class Result : Result<Empty>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Exception? Exception { get; }

    protected Result(bool isSuccess, Exception? exception) :
        base(isSuccess, Empty.Value,exception)
    {
        IsSuccess = isSuccess;
        Exception = exception;
    }
    public static Result Success() => new(true, null);
    public static Result Failure(Exception exception) => new(false, exception);
    public static Result<T> Success<T>(T value) => new Result<T>(true, value, null);
    public static Result<T> Failure<T>(Exception exception) => new Result<T>(false, default, exception);


 
}