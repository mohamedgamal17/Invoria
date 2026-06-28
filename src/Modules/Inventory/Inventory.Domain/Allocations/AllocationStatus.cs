namespace Invoria.Inventory.Domain.Allocations;

public enum AllocationStatus
{
    Pending = 0,
    Failed = 5,
    Allocated = 10,
    Released = 20,
    Completed = 30
}
