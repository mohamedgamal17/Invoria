namespace Invoria.Inventory.Domain.Allocations;

public enum AllocationStatus
{
    Pending = 0,
    Failed = 5,
    Allocated = 10,
    Dispatched = 15,
    Released = 20
}
