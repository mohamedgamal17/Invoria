using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.CustomerManagement.Contracts.Dtos;

namespace Invoria.CustomerManagement.Application.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommand : ICommand<CustomerDto>
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public UpdateCustomerCommand(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

