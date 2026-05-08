using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Catalog.Contracts.Dtos;

namespace Invoria.Catalog.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : ICommand<ProductDto>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public UpdateProductCommand(string id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }
    }
}
