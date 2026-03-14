using MediatR;
using Microsoft.Extensions.DependencyInjection;
namespace Invoria.Application.Tests
{
    public abstract class RequestHandlerTestFixture : TestFixture
    {
        protected async Task<TResult> SendAsync<TResult>(IRequest<TResult> command)
        {
            var mediator =  ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(command);
        }
    }
}
