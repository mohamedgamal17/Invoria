using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.Infrastructure.Common;
using System.Net;

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

        var problem = ExceptionToProblemDetailsMapper.Map(exception, instance, null);

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

        var problem = ExceptionToProblemDetailsMapper.Map(exception, instance, null);

        return (problem.Status, Envelope<T>.Failure(problem));
    }
}
