using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Endpoints.Tests.Products;

public class ProductEndpointTestFixture : CatalogTestFixture
{
    protected ICatalogRepository<Product> ProductRepository { get; private set; } = null!;

    public ProductEndpointTestFixture()
    {
        ProductRepository = Scope.ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
    }
}

