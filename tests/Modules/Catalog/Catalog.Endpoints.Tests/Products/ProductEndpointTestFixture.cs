using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Endpoints.Tests.Products;

public class ProductEndpointTestFixture : CatalogTestFixture
{
    protected ICatalogRepository<Product> ProductRepository { get; private set; } = null!;
    protected IInventoryRepository<Batch> BatchRepository { get; private set; } = null!;

    public ProductEndpointTestFixture()
    {
        ProductRepository = Scope.ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
        BatchRepository = Scope.ServiceProvider.GetRequiredService<IInventoryRepository<Batch>>();
    }
}

