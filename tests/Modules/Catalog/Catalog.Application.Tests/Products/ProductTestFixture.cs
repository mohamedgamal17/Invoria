using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Application.Tests.Products
{
    public class ProductTestFixture : CatalogTestFixture
    {
        protected IMediator Mediator { get; }

        public ProductTestFixture()
        {
            Mediator = ServiceProvider.GetRequiredService<IMediator>();
        }
    }
}
