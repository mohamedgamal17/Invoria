using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Domain.Orders.DebtSummary;
using Invoria.Reporting.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Orders.Queries.GetCustomerDebtSummary;

public sealed class GetCustomerDebtSummaryQueryHandler
    : IApplicatonRequestHandler<GetCustomerDebtSummaryQuery, CustomerDebtOverviewDto>
{
    private readonly IReportingRepository<DebtSummaryBase> _debtSummaryRepository;

    public GetCustomerDebtSummaryQueryHandler(IReportingRepository<DebtSummaryBase> debtSummaryRepository)
    {
        _debtSummaryRepository = debtSummaryRepository;
    }

    public async Task<Result<CustomerDebtOverviewDto>> Handle(
        GetCustomerDebtSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await _debtSummaryRepository
            .AsQuerable()
            .AsNoTracking()
            .OfType<DebtCustomerSummary>()
            .SingleOrDefaultAsync(x => x.CustomerId == request.CustomerId, cancellationToken);

        if (summary is null)
        {
            return Result.Success(new CustomerDebtOverviewDto
            {
                CustomerId = request.CustomerId
            });
        }

        return Result.Success(new CustomerDebtOverviewDto
        {
            CustomerId = summary.CustomerId,
            TotalOutstanding = summary.TotalOutstanding,
            TotalPaid = summary.TotalPaid,
            TotalOrderValue = summary.TotalOrderValue,
            DebtOrderCount = summary.DebtOrderCount,
            PartiallyPaidCount = summary.PartiallyPaidCount,
            UnpaidCount = summary.UnpaidCount,
            OldestDebtDate = summary.OldestDebtDate,
            OldestDebtAmount = summary.OldestDebtAmount,
            ComputedAt = summary.ComputedAt
        });
    }
}
