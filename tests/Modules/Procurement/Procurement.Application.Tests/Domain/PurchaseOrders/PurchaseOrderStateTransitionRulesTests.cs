using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Contracts.PurchaseOrders;

namespace Invoria.Procurement.Application.Tests.Domain.PurchaseOrders;

[TestFixture]
public class PurchaseOrderStateTransitionRulesTests
{
    [TestCase(PurchaseState.Draft, PurchaseState.Submitted, true)]
    [TestCase(PurchaseState.Draft, PurchaseState.Cancelled, true)]
    [TestCase(PurchaseState.Draft, PurchaseState.Approved, false)]
    [TestCase(PurchaseState.Submitted, PurchaseState.Approved, true)]
    [TestCase(PurchaseState.Submitted, PurchaseState.Reopened, true)]
    [TestCase(PurchaseState.Submitted, PurchaseState.Rejected, true)]
    [TestCase(PurchaseState.Submitted, PurchaseState.Cancelled, true)]
    [TestCase(PurchaseState.Approved, PurchaseState.Completed, true)]
    [TestCase(PurchaseState.Approved, PurchaseState.Reopened, true)]
    [TestCase(PurchaseState.Approved, PurchaseState.Cancelled, true)]
    [TestCase(PurchaseState.Approved, PurchaseState.Rejected, false)]
    [TestCase(PurchaseState.Reopened, PurchaseState.Submitted, true)]
    [TestCase(PurchaseState.Reopened, PurchaseState.Cancelled, true)]
    [TestCase(PurchaseState.Reopened, PurchaseState.Approved, false)]
    [TestCase(PurchaseState.Completed, PurchaseState.Draft, false)]
    [TestCase(PurchaseState.Cancelled, PurchaseState.Draft, false)]
    [TestCase(PurchaseState.Rejected, PurchaseState.Draft, false)]
    public void CanTransition_returns_expected(PurchaseState current, PurchaseState next, bool expected)
    {
        var result = PurchaseOrderStateTransitionRules.CanTransition(current, next);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void CanTransition_same_state_is_false()
    {
        Assert.That(
            PurchaseOrderStateTransitionRules.CanTransition(PurchaseState.Draft, PurchaseState.Draft),
            Is.False);
    }
}
