using FastEndpoints;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.BuildingBlocks.Infrastructure.Results;

namespace Invoria.BuildingBlocks.Infrastructure.Endpoints;

public abstract class EndpointBase<TRequest, TResponse> : Endpoint<TRequest, Envelope<TResponse>>
    where TRequest : notnull
{
    private readonly IResultToHttpMapper _resultMapper;

    protected EndpointBase(IResultToHttpMapper resultMapper)
    {
        _resultMapper = resultMapper;
    }

    protected async Task SendResultAsync(Result<TResponse> result, CancellationToken ct = default)
    {
        var (statusCode, body) = _resultMapper.Map(result, HttpContext?.Request.Path.Value);

        await Send.ResponseAsync(body, statusCode);
    }
}

public abstract class EndpointBase<TRequest> : Endpoint<TRequest, Envelope>
    where TRequest : notnull
{
    private readonly IResultToHttpMapper _resultMapper;

    protected EndpointBase(IResultToHttpMapper resultMapper)
    {
        _resultMapper = resultMapper;
    }

    protected async Task SendResultAsync(Result result, CancellationToken ct = default)
    {
        var (statusCode, body) = _resultMapper.Map(result, HttpContext?.Request.Path.Value);

        await Send.ResponseAsync(body, statusCode);
    }
}
