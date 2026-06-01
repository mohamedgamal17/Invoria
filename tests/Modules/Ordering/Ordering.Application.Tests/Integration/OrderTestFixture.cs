using Autofac;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using MediatR;

namespace Invoria.Ordering.Application.Tests.Integration
{
    public class OrderTestFixture : OrderingTestFixture
    {
        protected IMediator Mediator => Scope.Resolve<IMediator>();

        protected IOrderingRepository<Order> OrderRepository => Scope.Resolve<IOrderingRepository<Order>>();
    }
}
