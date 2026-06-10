using FluentAssertions;
using Invoria.Inventory.Domain.Returns;
using ContractReturnStatus = Invoria.Inventory.Contracts.Returns.Enums.ReturnStatus;

namespace Invoria.Inventory.Application.Tests.Domain.Returns;

[TestFixture]
public class ReturnTests
{
    [Test]
    public void Create_should_create_return_with_lines()
    {
        var returnLines = new[]
        {
            ReturnLine.Create("order-item-1", "product-1", 3),
            ReturnLine.Create("order-item-2", "product-2", 5)
        };

        var @return = TestReturn.Create(returnLines);

        @return.Type.Should().Be(ReturnType.Immediate);
        @return.Status.Should().Be(ContractReturnStatus.Pending);
        @return.ReturnLines.Should().HaveCount(2);
        @return.ReturnLines.Should().ContainSingle(l =>
            l.OrderItemId == "order-item-1" && l.ProductId == "product-1" && l.Quantity == 3);
        @return.ReturnLines.Should().ContainSingle(l =>
            l.OrderItemId == "order-item-2" && l.ProductId == "product-2" && l.Quantity == 5);
        @return.ReturnLines.Should().OnlyContain(l => l.ReturnId == @return.Id);
    }

    [Test]
    public void Create_should_throw_when_lines_are_empty()
    {
        var act = () => TestReturn.Create(Array.Empty<ReturnLine>());

        act.Should().Throw<ArgumentException>();
    }

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
