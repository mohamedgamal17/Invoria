using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Procurement.Domain.PurchaseOrders;

public class ReportPurchaseSalesMetrics : Entity
{
    public DateTimeOffset Date { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public ReportPeriod Period { get; private set; }

    private ReportPurchaseSalesMetrics()
    {
    }

    public ReportPurchaseSalesMetrics(
        DateTimeOffset date,
        decimal totalAmount,
        decimal subTotal,
        decimal taxAmount,
        decimal discountAmount,
        ReportPeriod period)
    {
        Date = date;
        TotalAmount = totalAmount;
        SubTotal = subTotal;
        TaxAmount = taxAmount;
        DiscountAmount = discountAmount;
        Period = period;
    }

    public void UpdateAmounts(
        decimal totalAmount,
        decimal subTotal,
        decimal taxAmount,
        decimal discountAmount)
    {
        TotalAmount = totalAmount;
        SubTotal = subTotal;
        TaxAmount = taxAmount;
        DiscountAmount = discountAmount;
    }
}