using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Modules.Catalog.Application.Tests.Products
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
