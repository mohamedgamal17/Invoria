using FluentAssertions;
using Invoria.Ordering.Application.Orders.Sagas;

namespace Invoria.Ordering.Application.Tests.Orders.Sagas;

[TestFixture]
public class OrderReturnSagaStateTests
{
    [Test]
    public void ApplyRequested_sets_order_id_and_requested_state()
    {
        var state = new OrderReturnSagaState();

        state.ApplyRequested("order-1");

        state.OrderId.Should().Be("order-1");
        state.State.Should().Be(OrderReturnSagaProcessState.Requested);
        state.ReturnId.Should().BeNull();
    }

    [Test]
    public void ApplyCompleted_sets_return_id_and_completed_state()
    {
        var state = new OrderReturnSagaState
        {
            OrderId = "order-1",
            State = OrderReturnSagaProcessState.Requested
        };

        state.ApplyCompleted("return-9");

        state.ReturnId.Should().Be("return-9");
        state.State.Should().Be(OrderReturnSagaProcessState.Completed);
        state.OrderId.Should().Be("order-1");
    }
}
