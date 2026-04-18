using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Endpoints.Customers.Requests
{
    public class ListCustomersRequest : PagingParams
    {
        [QueryParam]
        public string? Name { get; set; }
    }

    public class ListCustomersRequestValidator : AbstractValidator<ListCustomersRequest>
    {
        public ListCustomersRequestValidator()
        {
            Include(new PagingParamasValidator<ListCustomersRequest>());

            When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
            {
                RuleFor(x => x.Name!)
                    .MaximumLength(CustomerTableConsts.NameMaxLength);
            });
        }
    }
}

