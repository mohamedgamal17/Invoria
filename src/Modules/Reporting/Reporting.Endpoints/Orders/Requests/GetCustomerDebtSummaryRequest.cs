namespace Invoria.Reporting.Endpoints.Orders.Requests;

public sealed class GetCustomerDebtSummaryRequest
{
    public string CustomerId { get; set; } = string.Empty;
}
