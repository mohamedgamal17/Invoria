using Invoria.Application.Tests.Extensions;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Services;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Application.Tests.Assertions;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Integration.Commands;

[TestFixture]
public class CreateOrderCommandHandlerCustomerMappingTests : OrderTestFixture
{
    protected override void RegisterCustomerService(IServiceCollection services)
    {
        services.AddSingleton<ICustomerService, SyntheticListCustomerService>();
    }

    [Test]
    public async Task Should_attach_customer_dto_from_customer_service()
    {
        var customerId = Guid.NewGuid().ToString();
        var command = new CreateOrderCommand(
            customerId,
            new List<CreateOrderItemCommand>
            {
                new(Guid.NewGuid().ToString(), 2, 10m),
                new(Guid.NewGuid().ToString(), 1, 25m)
            });

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        var expectedCustomer = new CustomerDto
        {
            Id = customerId,
            Name = SyntheticListCustomerService.NameForId(customerId)
        };
        result.Value!.AssertOrderDto(command, ExpectedProduct, expectedCustomer);
    }

    private static ProductDto? ExpectedProduct(string id) => null;
}
