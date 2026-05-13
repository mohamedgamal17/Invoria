namespace Invoria.Reporting.Endpoints.Orders.Requests;

public sealed class GetOrderStatusSummaryRequest
{
    public DateTimeOffset? FromUtc { get; set; }

    public DateTimeOffset? ToUtc { get; set; }
}
