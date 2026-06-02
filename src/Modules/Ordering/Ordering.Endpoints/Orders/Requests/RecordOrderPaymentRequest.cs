using FastEndpoints;
using FluentValidation;
using Invoria.Ordering.Contracts.Orders.Enums;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class RecordOrderPaymentRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;

    public decimal PaidAmount { get; set; }

    public OrderPaymentMethod PaymentMethod { get; set; }

    public DateTimeOffset? PaidAt { get; set; }
}

public class RecordOrderPaymentRequestValidator : AbstractValidator<RecordOrderPaymentRequest>
{
    public RecordOrderPaymentRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PaidAmount).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}
