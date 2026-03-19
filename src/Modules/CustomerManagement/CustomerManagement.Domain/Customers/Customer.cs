using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.CustomerManagement.Domain.Customers
{
    public class Customer : AuditedAggregateRoot
    {
        public string Name { get; private set; } = default!;

        private Customer() { }

        public Customer(string name)
        {
            Name = name;
        }

        public void Update(string name)
        {
            Name = name;
        }
    }
}

