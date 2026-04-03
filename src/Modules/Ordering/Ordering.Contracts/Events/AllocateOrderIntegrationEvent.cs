using Invoria.Ordering.Contracts.Models;

namespace Invoria.Ordering.Contracts.Events
{
    public class AllocateOrderIntegrationEvent
    {
        public string Id { get; set; }
        public string OrderNumber { get; private set; }
        public string CustomerId { get; private set; }
        public List<OrderItemModel> Items { get; set; }
    }
}
