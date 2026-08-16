using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Ordering.Domain.Orders;

public class ReportOrderSalesProfitMetrics : Entity
{
    public DateTimeOffset Date { get; private set; }

    public decimal TotalRevenue { get; private set; }

    public decimal TotalCost { get; private set; }

    public decimal TotalProfit { get; private set; }

    public decimal TotalReturnAmount { get; private set; }

    public ReportPeriod Period { get; private set; }

    private ReportOrderSalesProfitMetrics()
    {
    }

    public ReportOrderSalesProfitMetrics(
        DateTimeOffset date,
        decimal totalRevenue,
        decimal totalCost,
        decimal totalProfit,
        decimal totalReturnAmount,
        ReportPeriod period)
    {
        Date = date;
        TotalRevenue = totalRevenue;
        TotalCost = totalCost;
        TotalProfit = totalProfit;
        TotalReturnAmount = totalReturnAmount;
        Period = period;
    }

    public void UpdateAmounts(
        decimal totalRevenue,
        decimal totalCost,
        decimal totalProfit,
        decimal totalReturnAmount)
    {
        TotalRevenue = totalRevenue;
        TotalCost = totalCost;
        TotalProfit = totalProfit;
        TotalReturnAmount = totalReturnAmount;
    }
}
