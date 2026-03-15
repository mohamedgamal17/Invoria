using System.Net;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Infrastructure.Common;

namespace Invoria.BuildingBlocks.Infrastructure.Results;

public static class ExceptionToProblemDetailsMapper
{
    public static ApiProblemDetails Map(Exception exception, string? instance = null, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var (statusCode, title, errorCode, errorKey) = exception is ApplicationExceptionBase appEx
            ? MapDomainException(appEx)
            : (500, "An unexpected error occurred.", "internal_error", (string?)null);

        var problem = new ApiProblemDetails
        {
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = exception.Message,
            Instance = instance,
            CorrelationId = correlationId,
            ErrorCode = errorCode,
            ErrorKey = errorKey
        };

        return problem;
    }

    private static (int StatusCode, string Title, string ErrorCode, string? ErrorKey) MapDomainException(ApplicationExceptionBase ex)
    {
        return ex switch
        {
            NotFoundException => (404, "Resource not found.", NotFoundException.DefaultCode, null),
            ConflictException => (409, "Conflict.", ConflictException.DefaultCode, null),
            BusinessLogicException => (422, "Business rule violated.", BusinessLogicException.DefaultCode, null),
            _ => (500, "An unexpected error occurred.", ex.Code, null)
        };
    }
}

