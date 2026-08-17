using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Ordering.Domain.Orders;

public class ReportOrderSalesMetrics : Entity
{
    public DateTimeOffset Date { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal TotalNetAmount { get; private set; }
    public decimal TotalReturnAmount { get; private set; }
    public ReportPeriod Period { get; private set; }

    private ReportOrderSalesMetrics() { }

    public ReportOrderSalesMetrics(DateTimeOffset date, decimal totalAmount, decimal totalNetAmount, decimal totalReturnAmount, ReportPeriod period)
    {
        Date = date;
        TotalAmount = totalAmount;
        TotalNetAmount = totalNetAmount;
        TotalReturnAmount = totalReturnAmount;
        Period = period;
    }

    public void UpdateAmounts(decimal totalAmount, decimal totalNetAmount, decimal totalReturnAmount)
    {
        TotalAmount = totalAmount;
        TotalNetAmount = totalNetAmount;
        TotalReturnAmount = totalReturnAmount;
    }
}