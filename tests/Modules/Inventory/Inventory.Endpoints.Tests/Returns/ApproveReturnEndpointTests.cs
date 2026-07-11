using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Inventory.Application.Returns.Commands.ApproveReturn;
using Invoria.Inventory.Application.Returns.Commands.CreateImmediateReturn;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ContractReturnStatus = Invoria.Inventory.Contracts.Returns.Enums.ReturnStatus;

namespace Invoria.Inventory.Endpoints.Tests.Returns;

[TestFixture]
public class ApproveReturnEndpointTests : InventoryTestFixture
{
    [Test]
    public async Task Should_approve_return()
    {
        var returnId = await CreatePendingReturnAsync();

        var response = await Client.PostAsync(
            $"/returns/{returnId}/approve",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<ReturnDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Id.Should().Be(returnId);
        envelope.Result.Status.Should().Be(ContractReturnStatus.Approved);
    }

    [Test]
    public async Task Should_return_not_found_when_return_missing()
    {
        var response = await Client.PostAsync(
            $"/returns/{Guid.NewGuid()}/approve",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Should_return_domain_error_when_return_not_pending()
    {
        var returnId = await CreatePendingReturnAsync();

        var firstApprove = await Client.PostAsync(
            $"/returns/{returnId}/approve",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        firstApprove.EnsureSuccessStatusCode();

        var response = await Client.PostAsync(
            $"/returns/{returnId}/approve",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
    }

    private async Task<string> CreatePendingReturnAsync()
    {
        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        var orderId = Guid.NewGuid().ToString();

        var createResult = await mediator.Send(new CreateImmediateReturnCommand
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

        await using var dbScope = Scope.ServiceProvider.CreateAsyncScope();
        var db = dbScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var returnEntity = await db.Set<ImmediateReturn>().SingleAsync(r => r.OrderId == orderId);
        return returnEntity.Id!;
    }
}
