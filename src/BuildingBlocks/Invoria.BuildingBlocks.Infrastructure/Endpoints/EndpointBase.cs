using FastEndpoints;
using FluentValidation;
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

        await SendAsync(body, statusCode);
    }

    protected void ValidateRequest(TRequest req)
    {
        var validator = Resolve<IValidator<TRequest>>();
        var validationResult = validator.Validate(req);

        if (validationResult.IsValid)
        {
            return;
        }

        foreach (var failure in validationResult.Errors)
        {
            AddError(failure.PropertyName, failure.ErrorMessage);
        }

        ThrowIfAnyErrors();
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

        await SendAsync(body, statusCode);
    }

    protected void ValidateRequest(TRequest req)
    {
        var validator = Resolve<IValidator<TRequest>>();
        var validationResult = validator.Validate(req);

        if (validationResult.IsValid)
        {
            return;
        }

        foreach (var failure in validationResult.Errors)
        {
            AddError(failure.PropertyName, failure.ErrorMessage);
        }

        ThrowIfAnyErrors();
    }
}
