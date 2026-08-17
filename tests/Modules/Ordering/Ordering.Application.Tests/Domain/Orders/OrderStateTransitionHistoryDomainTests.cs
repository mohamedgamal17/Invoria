using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderStateTransitionHistoryDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    [Test]
    public void Accept_records_transition_from_pending_to_processing()
    {
        var order = new Order("TR-ACCEPT", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-trace-accept");

        order.Accept();

        var transition = order.StateTransitionHistory.Should().ContainSingle().Subject;
        transition.OrderId.Should().Be("order-trace-accept");
        transition.FromStatus.Should().Be(OrderStatus.Pending);
        transition.ToStatus.Should().Be(OrderStatus.Processing);
        transition.ChangedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Accept_records_transition_from_revision_to_processing()
    {
        var order = new Order("TR-2", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-trace-accept-revision");
        typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(order, OrderStatus.Revision);

        order.Accept();

        var transition = order.StateTransitionHistory.Should().ContainSingle().Subject;
        transition.FromStatus.Should().Be(OrderStatus.Revision);
        transition.ToStatus.Should().Be(OrderStatus.Processing);
    }

    [Test]
    public void Revise_records_transition_from_processing_to_revision()
    {
        var order = new Order("TR-3", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-trace-revise");
        order.Accept();

        order.Revise();

        order.StateTransitionHistory.Should().HaveCount(2);
        var transition = order.StateTransitionHistory.Last();
        transition.FromStatus.Should().Be(OrderStatus.Processing);
        transition.ToStatus.Should().Be(OrderStatus.Revision);
    }

    [Test]
    public void RequestRevision_records_transition_from_processing_to_revision_pending()
    {
        var order = new Order("TR-4", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-trace-request-revision");
        order.Accept();
        order.MarkAsAllocated();

        order.RequestRevision();

        order.StateTransitionHistory.Should().HaveCount(2);
        var transition = order.StateTransitionHistory.Last();
        transition.FromStatus.Should().Be(OrderStatus.Processing);
        transition.ToStatus.Should().Be(OrderStatus.RevisionPending);
    }

    [Test]
    public void Cancel_records_transition_from_pending_to_cancelled()
    {
        var order = new Order("TR-5", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-trace-cancel");

        order.Cancel();

        var transition = order.StateTransitionHistory.Should().ContainSingle().Subject;
        transition.FromStatus.Should().Be(OrderStatus.Pending);
        transition.ToStatus.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Complete_records_transition_from_processing_to_completed()
    {
        var order = new Order("TR-6", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-trace-complete");
        order.Accept();

        order.Complete([]);

        order.StateTransitionHistory.Should().HaveCount(2);
        var transition = order.StateTransitionHistory.Last();
        transition.FromStatus.Should().Be(OrderStatus.Processing);
        transition.ToStatus.Should().Be(OrderStatus.Completed);
    }

    [Test]
    public void Full_lifecycle_records_one_entry_per_transition()
    {
        var order = new Order("TR-7", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-trace-lifecycle");

        order.Accept();
        order.Revise();
        order.Accept();
        order.Complete([]);

        order.StateTransitionHistory.Should().HaveCount(4);
        var story = order.StateTransitionHistory.OrderBy(t => t.ChangedAt).ToList();
        story[0].FromStatus.Should().Be(OrderStatus.Pending);
        story[0].ToStatus.Should().Be(OrderStatus.Processing);
        story[1].FromStatus.Should().Be(OrderStatus.Processing);
        story[1].ToStatus.Should().Be(OrderStatus.Revision);
        story[2].FromStatus.Should().Be(OrderStatus.Revision);
        story[2].ToStatus.Should().Be(OrderStatus.Processing);
        story[3].FromStatus.Should().Be(OrderStatus.Processing);
        story[3].ToStatus.Should().Be(OrderStatus.Completed);
    }

    [Test]
    public void Non_status_actions_do_not_record_transitions()
    {
        var order = new Order("TR-8", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-trace-no-transition");
        order.Accept();
        order.Complete([]);

        order.RecordPayment(20m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow);
        order.RecordInvoice("inv-1");
        order.RecordReturn("ret-1");

        order.StateTransitionHistory.Should().HaveCount(2);
        var transitions = order.StateTransitionHistory.OrderBy(t => t.ChangedAt).ToList();
        transitions[0].ToStatus.Should().Be(OrderStatus.Processing);
        transitions[1].ToStatus.Should().Be(OrderStatus.Completed);
    }

    [Test]
    public void Invalid_transition_does_not_record_history()
    {
        var order = new Order("TR-9", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-trace-invalid");

        var act = () => order.Complete([]);

        act.Should().Throw<InvalidOperationException>();
        order.StateTransitionHistory.Should().BeEmpty();
    }
}