using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Reporting.Contracts.Orders.Reports;
using Invoria.Reporting.Domain.Orders.DebtSummary;
using Invoria.Reporting.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Reporting.Application.Orders.Queries.GetDebtOverview;

public sealed class GetDebtOverviewQueryHandler
    : IApplicatonRequestHandler<GetDebtOverviewQuery, DebtOverviewDto>
{
    private readonly IReportingRepository<DebtSummaryBase> _debtSummaryRepository;

    public GetDebtOverviewQueryHandler(IReportingRepository<DebtSummaryBase> debtSummaryRepository)
    {
        _debtSummaryRepository = debtSummaryRepository;
    }

    public async Task<Result<DebtOverviewDto>> Handle(
        GetDebtOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var global = await _debtSummaryRepository
            .AsQuerable()
            .AsNoTracking()
            .OfType<DebtGlobalSummary>()
            .SingleOrDefaultAsync(cancellationToken);

        if (global is null)
        {
            return Result.Success(new DebtOverviewDto());
        }

        return Result.Success(new DebtOverviewDto
        {
            TotalOutstanding = global.TotalOutstanding,
            TotalPaid = global.TotalPaid,
            TotalOrderValue = global.TotalOrderValue,
            DebtOrderCount = global.DebtOrderCount,
            PartiallyPaidCount = global.PartiallyPaidCount,
            UnpaidCount = global.UnpaidCount,
            CollectionRate = global.CollectionRate,
            ComputedAt = global.ComputedAt
        });
    }
}
