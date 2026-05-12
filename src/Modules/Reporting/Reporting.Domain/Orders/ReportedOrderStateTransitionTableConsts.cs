namespace Invoria.Reporting.Domain.Orders;

public static class ReportedOrderStateTransitionTableConsts
{
    public const string TableName = "ReportedOrderStateTransition";

    public const int IdMaxLength = 256;
    public const int ReportedOrderIdMaxLength = 256;
    public const int ReasonMaxLength = 1024;
}
