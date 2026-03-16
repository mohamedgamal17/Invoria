using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Catalog.Application.Products.Queries.GetProductById;
using Invoria.Catalog.Application.Tests.Assertions;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Application.Tests.Products
{
    [TestFixture]
    public class GetProductByIdQueryHandlerTests : ProductTestFixture
    {
        protected ICatalogRepository<Product> ProductRepository { get; }

        public GetProductByIdQueryHandlerTests()
        {
            ProductRepository = ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
        }

        [Test]
        public async Task Should_return_product_when_found()
        {
            // Arrange
            var product = new Product("Test Product", "TEST-CODE", 10);
            await ProductRepository.Add(product);

            var query = new GetProductByIdQuery { Id = product.Id };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.ShouldBeSuccess();
            result.Value!.AssertProductDto(product);
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
