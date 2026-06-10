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
    public void ApplyCreated_sets_return_id_and_created_state()
    {
        var state = new OrderReturnSagaState
        {
            OrderId = "order-1",
            State = OrderReturnSagaProcessState.Requested
        };

        state.ApplyCreated("return-9");

        state.ReturnId.Should().Be("return-9");
        state.State.Should().Be(OrderReturnSagaProcessState.Created);
        state.OrderId.Should().Be("order-1");
    }
}
