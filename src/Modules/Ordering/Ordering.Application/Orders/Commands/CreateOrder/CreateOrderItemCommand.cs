namespace Invoria.Ordering.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderItemCommand
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public CreateOrderItemCommand(string productId, int quantity, decimal price)
        {
            ProductId = productId;
            Quantity = quantity;
            Price = price;
        }
    }
}
