using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Invoria.Inventory.Endpoints.Returns.Requests;

public sealed class ApproveReturnRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;
}

public sealed class ApproveReturnRequestValidator : AbstractValidator<ApproveReturnRequest>
{
    public ApproveReturnRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
