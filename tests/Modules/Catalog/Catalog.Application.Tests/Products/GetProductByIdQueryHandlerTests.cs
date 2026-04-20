using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Catalog.Application.Products.Queries.GetProductById;
using Invoria.Catalog.Application.Tests.Assertions;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Application.Tests.Products
{
    [TestFixture]
    public class GetProductByIdQueryHandlerTests : ProductTestFixture
    {
        protected ICatalogRepository<Product> ProductRepository { get; }
        protected IInventoryRepository<Batch> BatchRepository { get; }

        public GetProductByIdQueryHandlerTests()
        {
            ProductRepository = ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
            BatchRepository = ServiceProvider.GetRequiredService<IInventoryRepository<Batch>>();
        }

        [Test]
        public async Task Should_return_product_when_found()
        {
            // Arrange
            var product = new Product("Test Product", "TEST-CODE", 10);
            await ProductRepository.Add(product);
            await BatchRepository.Add(new Batch(product.Id, 15, 10m));

            var query = new GetProductByIdQuery { Id = product.Id };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.ShouldBeSuccess();
            result.Value!.AssertProductDto(product, 15, 0);
        }

        [Test]
        public async Task Should_return_failure_when_product_not_found()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid().ToString();
            var query = new GetProductByIdQuery { Id = nonExistentId };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Exception.Should().NotBeNull();
            result.Exception.Should().BeOfType<NotFoundException>();
        }
    }
}
