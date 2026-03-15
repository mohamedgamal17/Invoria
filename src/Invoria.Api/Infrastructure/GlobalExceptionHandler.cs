using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Diagnostics;

namespace Invoria.Api.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception has occurred while executing the request.");

        var problemDetails = ExceptionToProblemDetailsMapper.Map(
            exception,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);
        
        httpContext.Response.StatusCode = problemDetails.Status;

        var envelope = Envelope.Failure(problemDetails);

        await httpContext.Response.WriteAsJsonAsync(envelope, cancellationToken);

        return true;
    }
}
