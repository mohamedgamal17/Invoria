using FastEndpoints;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.Infrastructure;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;

namespace Invoria.Api.Endpoints.Demo;

public sealed class GetDemoEndpoint : EndpointBase<GetDemoRequest, GetDemoResponse>
{
    public GetDemoEndpoint(IResultToHttpMapper mapper) : base(mapper)
    {
    }

    public override void Configure()
    {
        Get("/demo");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Demo endpoint that returns a Result<TResponse> mapped by the shared base endpoint.";
        });
    }

    public override async Task HandleAsync(GetDemoRequest req, CancellationToken ct)
    {
        // In real code you would call a command/query handler here.
        Result<GetDemoResponse> result = Result.Success(new GetDemoResponse("Hello from Invoria via FastEndpoints!"));


        await SendResultAsync(result, ct);
    }
}

public sealed record GetDemoRequest { }              

public sealed record GetDemoResponse(string Message);

  