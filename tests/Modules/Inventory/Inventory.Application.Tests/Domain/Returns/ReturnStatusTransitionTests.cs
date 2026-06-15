using FluentAssertions;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Domain.Returns.Events;
using ContractReturnStatus = Invoria.Inventory.Contracts.Returns.Enums.ReturnStatus;

namespace Invoria.Inventory.Application.Tests.Domain.Returns;

[TestFixture]
public class ReturnStatusTransitionTests
{
    [Test]
    public void Approve_sets_status_to_approved_when_pending()
    {
        var @return = CreateReturn();

        @return.Approve();

        @return.Status.Should().Be(ContractReturnStatus.Approved);
    }

    [Test]
    public void Reject_sets_status_to_rejected_when_pending()
    {
        var @return = CreateReturn();

        @return.Reject();

        @return.Status.Should().Be(ContractReturnStatus.Rejected);
    }

    [Test]
    public void Complete_sets_status_to_completed_when_approved()
    {
        var @return = CreateReturn();
        @return.Approve();

        @return.Complete();

        @return.Status.Should().Be(ContractReturnStatus.Completed);
    }

    [Test]
    public void Approve_should_raise_ReturnApprovedDomainEvent()
    {
        var @return = CreateReturn();

        @return.Approve();

        @return.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ReturnApprovedDomainEvent>();
        var ev = (ReturnApprovedDomainEvent)@return.DomainEvents.Single();
        ev.Return.Should().BeSameAs(@return);
        ev.Return.Status.Should().Be(ContractReturnStatus.Approved);
    }

    [Test]
    public void Approve_should_not_raise_domain_event_when_not_pending()
    {
        var @return = CreateReturn();
        @return.Approve();

        var act = () => @return.Approve();

        act.Should().Throw<InvalidOperationException>();
        @return.DomainEvents.OfType<ReturnApprovedDomainEvent>().Should().ContainSingle();
    }

    [Test]
    public void Approve_throws_when_not_pending()
    {
        var @return = CreateReturn();
        @return.Approve();

        var act = () => @return.Approve();

        act.Should().Throw<InvalidOperationException>();
        @return.Status.Should().Be(ContractReturnStatus.Approved);
    }

    [Test]
    public void Reject_throws_when_not_pending()
    {
        var @return = CreateReturn();
        @return.Approve();

        var act = () => @return.Reject();

        act.Should().Throw<InvalidOperationException>();
        @return.Status.Should().Be(ContractReturnStatus.Approved);
    }

    [Test]
    public void Complete_throws_when_not_approved()
    {
        var pendingReturn = CreateReturn();
        var actFromPending = () => pendingReturn.Complete();

        actFromPending.Should().Throw<InvalidOperationException>();
        pendingReturn.Status.Should().Be(ContractReturnStatus.Pending);

        var rejectedReturn = CreateReturn();
        rejectedReturn.Reject();
        var actFromRejected = () => rejectedReturn.Complete();

        actFromRejected.Should().Throw<InvalidOperationException>();
        rejectedReturn.Status.Should().Be(ContractReturnStatus.Rejected);
    }

    private static TestReturn CreateReturn() =>
        TestReturn.Create([ReturnLine.Create("order-item-1", "product-1", 1)]);

    private sealed class TestReturn : Return
    {
        public static TestReturn Create(IEnumerable<ReturnLine> returnLines) =>
            new(returnLines);

        private TestReturn(IEnumerable<ReturnLine> returnLines)
            : base(returnLines, ReturnType.Immediate)
        {
        }
    }
}
