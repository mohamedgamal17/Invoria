using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Catalog.Contracts.Dtos;

namespace Invoria.Catalog.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommand : ICommand<ProductDto>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public CreateProductCommand(string name, decimal price)
        {
            Name = name;
            Price = price;
        }
    }
}
