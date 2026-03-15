using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Modules.Catalog.Application.Products.Commands.UpdateProduct;
using Invoria.Modules.Catalog.Application.Tests.Assertions;
using Invoria.Modules.Catalog.Domain;
using Invoria.Modules.Catalog.Domain.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Modules.Catalog.Application.Tests.Products
{
    [TestFixture]
    public class UpdateProductCommandHandlerTests : ProductTestFixture
    {
        protected ICatalogRepository<Product> ProductRepository { get; }

        public UpdateProductCommandHandlerTests()
        {
            ProductRepository = ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
        }

        [Test]
        public async Task Should_update_product()
        {
            // Arrange
            var initialProduct = new Product("Old Name", "OLD-CODE", 10);
            await ProductRepository.Add(initialProduct);

            var command = new UpdateProductCommand(initialProduct.Id, "New Name", "NEW-CODE", 20);

            // Act
            var result = await Mediator.Send(command);

            // Assert
            result.ShouldBeSuccess();

            var updatedProduct = await ProductRepository.SingleOrDefault(x => x.Id == initialProduct.Id);

            updatedProduct.Should().NotBeNull();
            updatedProduct!.Name.Should().Be("New Name");
            updatedProduct.Code.Should().Be("NEW-CODE");
            updatedProduct.Price.Should().Be(20);

            result.Value!.AssertProductDto(updatedProduct);
        }

        [Test]
        public async Task Should_return_failure_when_product_not_found()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid().ToString();
            var command = new UpdateProductCommand(nonExistentId, "New Name", "NEW-CODE", 20);

            // Act
            var result = await Mediator.Send(command);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Exception.Should().NotBeNull();
            result.Exception.Should().BeOfType<NotFoundException>();
        }
    }
}
