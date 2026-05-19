using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;

namespace Invoria.Reporting.Endpoints.Orders.Requests;

public sealed class ListCustomerDebtOverviewRequestValidator : AbstractValidator<ListCustomerDebtOverviewRequest>
{
    public ListCustomerDebtOverviewRequestValidator()
    {
        Include(new PagingParamasValidator<ListCustomerDebtOverviewRequest>());
    }
}
