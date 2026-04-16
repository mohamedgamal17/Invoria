namespace Invoria.Procurement.Domain.PurchaseOrders;

public static class PurchaseOrderStateTransitionRules
{
    public static bool CanTransition(PurchaseState current, PurchaseState next)
    {
        if (current == next)
        {
            return false;
        }

        return current switch
        {
            PurchaseState.Draft => next is PurchaseState.Submitted or PurchaseState.Cancelled,
            PurchaseState.Submitted => next is PurchaseState.Approved or PurchaseState.Rejected
                or PurchaseState.Cancelled,
            PurchaseState.Approved => next is PurchaseState.Completed or PurchaseState.Cancelled,
            PurchaseState.Completed => false,
            PurchaseState.Cancelled => false,
            PurchaseState.Rejected => false,
            _ => false,
        };
    }
}
