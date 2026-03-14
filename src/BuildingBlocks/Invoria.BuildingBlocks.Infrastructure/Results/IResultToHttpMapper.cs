using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.Infrastructure.Common;

namespace Invoria.BuildingBlocks.Infrastructure.Results;

public interface IResultToHttpMapper
{
    (int StatusCode, Envelope ResponseBody) Map(Result result, string? instance = null);

    (int StatusCode, Envelope<T> ResponseBody) Map<T>(Result<T> result, string? instance = null);
}

