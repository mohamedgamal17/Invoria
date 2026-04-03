using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Integration
{
    public class OrderTestFixture : OrderingTestFixture
    {
        protected IMediator Mediator { get; }

        public OrderTestFixture()
        {
            Mediator = ServiceProvider.GetRequiredService<IMediator>();
        }
    }
}
