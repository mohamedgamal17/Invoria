using System.Net;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.Infrastructure.Common;

namespace Invoria.BuildingBlocks.Infrastructure.Results;

public class DefaultResultToHttpMapper : IResultToHttpMapper
{
    public (int StatusCode, Envelope ResponseBody) Map(Result result, string? instance = null)
    {
        if (result.IsSuccess)
        {
            return ((int)HttpStatusCode.OK, Envelope.Success());
        }

        var exception = result.Exception
                        ?? throw new InvalidOperationException("Failed Result must have a non-null Exception.");

        var (statusCode, title, code) = exception is ApplicationExceptionBase appEx
            ? MapDomainException(appEx)
            : (500, "An unexpected error occurred.", null);

        var problem = new ApiProblemDetails
        {
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = exception.Message,
            Instance = instance
        };

        if (!string.IsNullOrWhiteSpace(code))
        {
            problem.Errors["code"] = new[] { code };
        }

        return (problem.Status, Envelope.Failure(problem));
    }

    public (int StatusCode, Envelope<T> ResponseBody) Map<T>(Result<T> result, string? instance = null)
    {
        if (result.IsSuccess)
        {
            return ((int)HttpStatusCode.OK, Envelope<T>.Success(result.Value!));
        }

        var exception = result.Exception
                        ?? throw new InvalidOperationException("Failed Result<T> must have a non-null Exception.");

        var (statusCode, title, code) = exception is ApplicationExceptionBase appEx
            ? MapDomainException(appEx)
            : (500, "An unexpected error occurred.", null);

        var problem = new ApiProblemDetails
        {
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = exception.Message,
            Instance = instance
        };

        if (!string.IsNullOrWhiteSpace(code))
        {
            problem.Errors["code"] = new[] { code };
        }

        return (problem.Status, Envelope<T>.Failure(problem));
    }

    private static (int StatusCode, string Title, string Code) MapDomainException(ApplicationExceptionBase ex)
    {
        return ex switch
        {
            NotFoundException => (404, "Resource not found.", NotFoundException.DefaultCode),
            ConflictException => (409, "Conflict.", ConflictException.DefaultCode),
            BusinessLogicException => (422, "Business rule violated.", BusinessLogicException.DefaultCode),
            _ => (500, "An unexpected error occurred.", ex.Code)
        };
    }
}
