namespace Invoria.Ordering.Application.Orders.Sagas;

public static class OrderSagaProcessState
{
    public const string Created = "Created";

    public const string RequestAllocation = "RequestAllocation";

    public const string Allocate = "Allocate";

    public const string AllocationFailed = "AllocationFailed";

    public const string AllocationSucceeded = "AllocationSucceeded";

    public const string RevisionRequested = "RevisionRequested";
}
