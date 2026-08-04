using Invoria.BuildingBlocks.Application.Factories;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.Customers.Factories
{
    public class ReportCustomerMetricsResponseFactory : ResponseFactory<ReportCustomerMetrics, ReportCustomerMetricsPeriodDto>, IReportCustomerMetricsResponseFactory
    {
        public override Task<ReportCustomerMetricsPeriodDto> PrepareDto(ReportCustomerMetrics view)
        {
            var dto = new ReportCustomerMetricsPeriodDto
            {
                Date = view.Date,
                TotalCount = view.TotalCount,
                Period = view.Period
            };

            return Task.FromResult(dto);
        }

        public async Task<ReportCustomerMetricsDto> PrepareMetricsDto(ReportCustomerMetrics report)
        {
            var periodDto = await PrepareDto(report);

            var dto = new ReportCustomerMetricsDto();

            switch (report.Period)
            {
                case ReportPeriod.Daily:
                    dto.ThisDay = periodDto;
                    break;
                case ReportPeriod.Monthly:
                    dto.ThisMonth = periodDto;
                    break;
                case ReportPeriod.Yearly:
                    dto.ThisYear = periodDto;
                    break;
                case ReportPeriod.AllTheTime:
                    dto.AllTime = periodDto;
                    break;
            }

            return dto;
        }
    }
}
