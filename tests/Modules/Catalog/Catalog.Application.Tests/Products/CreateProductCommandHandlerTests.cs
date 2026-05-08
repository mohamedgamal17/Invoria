using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Catalog.Application.Products.Commands.CreateProduct;
using Invoria.Catalog.Application.Tests.Assertions;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Microsoft.Extensions.DependencyInjection;
namespace Invoria.Catalog.Application.Tests.Products
{
    [TestFixture]
    public  class CreateProductCommandHandlerTests : ProductTestFixture
    {
        protected ICatalogRepository<Product> ProductRepository { get; }

        public CreateProductCommandHandlerTests()
        {
            ProductRepository = ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
        }

        [Test]
        public async Task Should_create_product()
        {
            var command = new CreateProductCommand(Guid.NewGuid().ToString(), 50);

            var result = await Mediator.Send(command);

            result.ShouldBeSuccess();

            var product = await ProductRepository.SingleOrDefault(x => x.Id == result.Value.Id);

            product.Should().NotBeNull();

            result.Value!.AssertProductDto(product);
        }
    }
}
