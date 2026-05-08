using FluentAssertions;
using Invoria.Catalog.Application.Products.Commands.CreateProduct;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Application.Tests.Assertions
{
    public static class ProductAssertionExtensions
    {
        public static void AssertCreateProductCommand(this Product product , CreateProductCommand command)
        {
            product.Name.Should().Be(command.Name);
            product.Price.Should().Be(command.Price);
        } 

        public static void AssertProductDto(this ProductDto dto , Product product)
        {
            dto.Id.Should().Be(product.Id);
            dto.Name.Should().Be(product.Name);
            dto.Price.Should().Be(product.Price);
            dto.Stock.Should().NotBeNull();
            dto.Stock!.ActualQuantity.Should().Be(0);
            dto.Stock.ReservedQuantity.Should().Be(0);
        }

        public static void AssertProductDto(
            this ProductDto dto,
            Product product,
            int expectedActualQuantity,
            int expectedReservedQuantity)
        {
            dto.Id.Should().Be(product.Id);
            dto.Name.Should().Be(product.Name);
            dto.Price.Should().Be(product.Price);
            dto.Stock.Should().NotBeNull();
            dto.Stock!.ActualQuantity.Should().Be(expectedActualQuantity);
            dto.Stock.ReservedQuantity.Should().Be(expectedReservedQuantity);
        }
    }
}
