using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Inventory.Application.Returns.Commands.ApproveReturn;
using Invoria.Inventory.Application.Returns.Commands.CreateImmediateReturn;
using Invoria.Inventory.Application.Tests.Batches;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ContractReturnStatus = Invoria.Inventory.Contracts.Returns.Enums.ReturnStatus;

namespace Invoria.Inventory.Application.Tests.Returns;

[TestFixture]
public class ApproveReturnCommandHandlerTests : BatchTestFixture
{
    [Test]
    public async Task Should_approve_pending_return()
    {
        var orderId = Guid.NewGuid().ToString();
        var createResult = await Mediator.Send(new CreateImmediateReturnCommand
        {
            OrderId = orderId,
            AllocationId = Guid.NewGuid().ToString(),
            Lines =
            [
                new CreateImmediateReturnLineItem
                {
                    OrderItemId = $"oi-{Guid.NewGuid():N}",
                    ProductId = Guid.NewGuid().ToString(),
                    Quantity = 2
                }
            ]
        });

        createResult.IsSuccess.Should().BeTrue();

        string returnId;
        await using (var scope = ServiceProvider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            returnId = (await db.Set<ImmediateReturn>().SingleAsync(r => r.OrderId == orderId)).Id!;
        }

        var result = await Mediator.Send(new ApproveReturnCommand(returnId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(returnId);
        result.Value.Status.Should().Be(ContractReturnStatus.Approved);
        result.Value.ReturnLines.Should().HaveCount(1);
    }

    [Test]
    public async Task Should_return_not_found_when_return_missing()
    {
        var result = await Mediator.Send(new ApproveReturnCommand(Guid.NewGuid().ToString()));

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NotFoundException>();
    }

    [Test]
    public async Task Should_return_business_logic_failure_when_not_pending()
    {
        var orderId = Guid.NewGuid().ToString();
        await Mediator.Send(new CreateImmediateReturnCommand
        {
            OrderId = orderId,
            AllocationId = Guid.NewGuid().ToString(),
            Lines =
            [
                new CreateImmediateReturnLineItem
                {
                    OrderItemId = $"oi-{Guid.NewGuid():N}",
                    ProductId = Guid.NewGuid().ToString(),
                    Quantity = 1
                }
            ]
        });

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var returnId = (await db.Set<ImmediateReturn>().SingleAsync(r => r.OrderId == orderId)).Id!;

        (await Mediator.Send(new ApproveReturnCommand(returnId))).IsSuccess.Should().BeTrue();

        var result = await Mediator.Send(new ApproveReturnCommand(returnId));

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<BusinessLogicException>();
    }
}
