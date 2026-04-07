namespace Invoria.Ordering.Contracts.Events;

/// <summary>
/// Why Ordering requested inventory to release order line allocations (same transport message, different completion handlers).
/// </summary>
public enum AllocationReleaseReason
{
    Reopen = 0,
    Refusal = 1,
}
