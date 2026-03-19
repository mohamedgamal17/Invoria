using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;

namespace Invoria.CustomerManagement.Endpoints.Customers.Requests
{
    public class ListCustomersRequest : PagingParams
    {
    }

    public class ListCustomersRequestValidator : AbstractValidator<ListCustomersRequest>
    {
        public ListCustomersRequestValidator()
        {
            Include(new PagingParamasValidator<ListCustomersRequest>());
        }
    }
}

