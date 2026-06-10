namespace Invoria.Ordering.Application.Orders.Sagas;

public static class OrderReturnSagaProcessState
{
    public const string Requested = "Requested";

    public const string Created = "Created";

    public const string Completed = "Completed";
}
