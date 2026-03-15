using FluentAssertions;
using Invoria.Modules.Catalog.Application.Products.Commands.CreateProduct;
using Invoria.Modules.Catalog.Contracts.Dtos;
using Invoria.Modules.Catalog.Domain.Products;

namespace Invoria.Modules.Catalog.Application.Tests.Assertions
{
    public static class ProductAssertionExtensions
    {
        public static void AssertCreateProductCommand(this Product product , CreateProductCommand command)
        {
            product.Name.Should().Be(command.Name);
            product.Code.Should().Be(command.Code);
            product.Price.Should().Be(command.Price);
        } 

        public static void AssertProductDto(this ProductDto dto , Product product)
        {
            dto.Id.Should().Be(product.Id);
            dto.Name.Should().Be(product.Name);
            dto.Code.Should().Be(product.Code);
            dto.Price.Should().Be(product.Price);
        }
    }
}
