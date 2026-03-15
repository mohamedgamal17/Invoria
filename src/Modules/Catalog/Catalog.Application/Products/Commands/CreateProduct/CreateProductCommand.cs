using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Modules.Catalog.Contracts.Dtos;

namespace Invoria.Modules.Catalog.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommand : ICommand<ProductDto>
    {
        public string Name { get; set; }
        public string? Code { get; set; }
        public decimal Price { get; set; }
        public CreateProductCommand(string name, string? code, decimal price)
        {
            Name = name;
            Code = code;
            Price = price;
        }
    }
}
