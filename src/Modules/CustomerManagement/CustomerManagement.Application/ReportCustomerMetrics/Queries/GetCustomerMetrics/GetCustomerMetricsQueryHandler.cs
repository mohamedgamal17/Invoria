using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.CustomerManagement.Application.ReportCustomerMetrics.Factories;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.ReportCustomerMetrics.Queries.GetCustomerMetrics
{
    using ReportCustomerMetricsEntity = Invoria.CustomerManagement.Domain.Customers.ReportCustomerMetrics;

    public class GetCustomerMetricsQueryHandler : IApplicatonRequestHandler<GetCustomerMetricsQuery, ReportCustomerMetricsDto>
    {
        private readonly ICustomerRepository<ReportCustomerMetricsEntity> _reportCustomerMetricsRepository;
        private readonly IReportCustomerMetricsResponseFactory _reportCustomerMetricsResponseFactory;

        public GetCustomerMetricsQueryHandler(
            ICustomerRepository<ReportCustomerMetricsEntity> reportCustomerMetricsRepository,
            IReportCustomerMetricsResponseFactory reportCustomerMetricsResponseFactory)
        {
            _reportCustomerMetricsRepository = reportCustomerMetricsRepository;
            _reportCustomerMetricsResponseFactory = reportCustomerMetricsResponseFactory;
        }

        public async Task<Result<ReportCustomerMetricsDto>> Handle(GetCustomerMetricsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.Now;

            var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
            var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
            var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

            var daily = await _reportCustomerMetricsRepository.SingleOrDefault(
                x => x.Period == ReportPeriod.Daily && x.Date == todayStart, cancellationToken);
            var monthly = await _reportCustomerMetricsRepository.SingleOrDefault(
                x => x.Period == ReportPeriod.Monthly && x.Date == monthStart, cancellationToken);
            var yearly = await _reportCustomerMetricsRepository.SingleOrDefault(
                x => x.Period == ReportPeriod.Yearly && x.Date == yearStart, cancellationToken);
            var allTime = await _reportCustomerMetricsRepository.SingleOrDefault(
                x => x.Period == ReportPeriod.AllTheTime, cancellationToken);

            daily ??= new ReportCustomerMetricsEntity(todayStart, 0, ReportPeriod.Daily);
            monthly ??= new ReportCustomerMetricsEntity(monthStart, 0, ReportPeriod.Monthly);
            yearly ??= new ReportCustomerMetricsEntity(yearStart, 0, ReportPeriod.Yearly);
            allTime ??= new ReportCustomerMetricsEntity(DateTimeOffset.MinValue, 0, ReportPeriod.AllTheTime);

            var dto = await _reportCustomerMetricsResponseFactory.PrepareMetricsDto(daily, monthly, yearly, allTime);

            return dto;
        }
    }
}
