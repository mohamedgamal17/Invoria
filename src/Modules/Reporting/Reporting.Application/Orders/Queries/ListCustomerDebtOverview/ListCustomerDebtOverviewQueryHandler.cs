using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Domain.Orders.DebtSummary;
using Invoria.Reporting.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Orders.Queries.ListCustomerDebtOverview;

public sealed class ListCustomerDebtOverviewQueryHandler
    : IApplicatonRequestHandler<ListCustomerDebtOverviewQuery, PagingDto<CustomerDebtOverviewDto>>
{
    private readonly IReportingRepository<DebtSummaryBase> _debtSummaryRepository;

    public ListCustomerDebtOverviewQueryHandler(IReportingRepository<DebtSummaryBase> debtSummaryRepository)
    {
        _debtSummaryRepository = debtSummaryRepository;
    }

    public async Task<Result<PagingDto<CustomerDebtOverviewDto>>> Handle(
        ListCustomerDebtOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var query = _debtSummaryRepository
            .AsQuerable()
            .AsNoTracking()
            .OfType<DebtCustomerSummary>()
            .OrderByDescending(x => x.TotalOutstanding)
            .ThenBy(x => x.CustomerId)
            .Select(x => new CustomerDebtOverviewDto
            {
                CustomerId = x.CustomerId,
                TotalOutstanding = x.TotalOutstanding,
                TotalPaid = x.TotalPaid,
                TotalOrderValue = x.TotalOrderValue,
                DebtOrderCount = x.DebtOrderCount,
                PartiallyPaidCount = x.PartiallyPaidCount,
                UnpaidCount = x.UnpaidCount,
                OldestDebtDate = x.OldestDebtDate,
                OldestDebtAmount = x.OldestDebtAmount,
                ComputedAt = x.ComputedAt
            });

        var paging = await query.ToPaged(request.Skip, request.Length);

        return Result.Success(paging);
    }
}
