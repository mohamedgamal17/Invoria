using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.CustomerManagement.Contracts.Dtos;

namespace Invoria.CustomerManagement.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommand : ICommand<CustomerDto>
    {
        public string Name { get; set; }

        public CreateCustomerCommand(string name)
        {
            Name = name;
        }
    }
}

