using FluentAssertions;
using Invoria.Inventory.Application.Returns.Commands.CreateImmediateReturn;
using Invoria.Inventory.Application.Tests.Batches;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ContractReturnStatus = Invoria.Inventory.Contracts.Returns.Enums.ReturnStatus;

namespace Invoria.Inventory.Application.Tests.Returns;

[TestFixture]
public class CreateImmediateReturnCommandHandlerTests : BatchTestFixture
{
    [Test]
    public async Task Should_create_immediate_return_with_lines()
    {
        var orderId = Guid.NewGuid().ToString();
        var allocationId = Guid.NewGuid().ToString();
        var orderItemId = $"oi-{Guid.NewGuid():N}";
        var productId = Guid.NewGuid().ToString();

        var command = new CreateImmediateReturnCommand
        {
            OrderId = orderId,
            AllocationId = allocationId,
            Lines =
            [
                new CreateImmediateReturnLineItem
                {
                    OrderItemId = orderItemId,
                    ProductId = productId,
                    Quantity = 4
                }
            ]
        };

        var result = await Mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var immediateReturn = await db.Set<ImmediateReturn>()
            .Include(r => r.ReturnLines)
            .SingleAsync(r => r.OrderId == orderId);

        immediateReturn.AllocationId.Should().Be(allocationId);
        immediateReturn.Status.Should().Be(ContractReturnStatus.Pending);
        immediateReturn.ReturnLines.Should().ContainSingle();
        immediateReturn.ReturnLines.Single().OrderItemId.Should().Be(orderItemId);
        immediateReturn.ReturnLines.Single().ProductId.Should().Be(productId);
        immediateReturn.ReturnLines.Single().Quantity.Should().Be(4);
    }
}
