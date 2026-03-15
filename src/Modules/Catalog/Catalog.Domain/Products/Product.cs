using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Modules.Catalog.Domain.Products
{
    public class Product : AuditedAggregateRoot
    {
        public string Name { get; private set; }
        public string? Code { get; private set; }
        public decimal Price { get; private set; }


        //for efcore
        private Product() { }

        public Product(string name, string? code, decimal price)
        {
            Name = name;
            Code = code;
            Price = price;
        }

        public void Update(string name, string? code, decimal price)
        {
            Name = name;
            Code = code;
            Price = price;
        }
    } 


    public static class ProductTableConsts
    {
        public const string TableName = "Products";

        public const int IdMaxLength = 256;

        public const int NameMaxLength = 500;

        public const int CodeMaxLength = 256;

    }
}
