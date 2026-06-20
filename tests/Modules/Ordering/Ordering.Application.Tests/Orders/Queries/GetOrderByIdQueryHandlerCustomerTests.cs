using Invoria.Application.Tests.Extensions;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Services;
using Invoria.Ordering.Application.Orders.Queries.GetOrderById;
using Invoria.Ordering.Application.Tests.Assertions;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Orders.Queries;

[TestFixture]
public class GetOrderByIdQueryHandlerCustomerTests : OrderTestFixture
{
    private IOrderingRepository<Order> OrderRepository =>
        ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();

    protected override void RegisterCustomerService(IServiceCollection services)
    {
        services.AddSingleton<ICustomerService, SyntheticListCustomerService>();
    }

    protected override async Task BeforeAnyTestRunAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_include_customer_when_customer_service_returns_match()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();

        var query = new GetOrderByIdQuery { Id = order.Id };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var expected = new CustomerDto
        {
            Id = order.CustomerId,
            Name = SyntheticListCustomerService.NameForId(order.CustomerId)
        };
        result.Value!.AssertOrderDto(order, expected);
    }
}
