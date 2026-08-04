using Invoria.BuildingBlocks.Application.Factories;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.ReportCustomerMetrics.Factories
{
    using ReportCustomerMetricsEntity = Invoria.CustomerManagement.Domain.Customers.ReportCustomerMetrics;

    public class ReportCustomerMetricsResponseFactory : ResponseFactory<ReportCustomerMetricsEntity, ReportCustomerMetricsPeriodDto>, IReportCustomerMetricsResponseFactory
    {
        public override Task<ReportCustomerMetricsPeriodDto> PrepareDto(ReportCustomerMetricsEntity view)
        {
            var dto = new ReportCustomerMetricsPeriodDto
            {
                Date = view.Date,
                TotalCount = view.TotalCount,
                Period = view.Period
            };

            return Task.FromResult(dto);
        }

        public async Task<ReportCustomerMetricsDto> PrepareMetricsDto(ReportCustomerMetricsEntity report)
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

        public async Task<ReportCustomerMetricsDto> PrepareMetricsDto(
            ReportCustomerMetricsEntity? daily,
            ReportCustomerMetricsEntity? monthly,
            ReportCustomerMetricsEntity? yearly,
            ReportCustomerMetricsEntity? allTime)
        {
            var dto = new ReportCustomerMetricsDto();

            if (daily is not null)
            {
                dto.ThisDay = await PrepareDto(daily);
            }

            if (monthly is not null)
            {
                dto.ThisMonth = await PrepareDto(monthly);
            }

            if (yearly is not null)
            {
                dto.ThisYear = await PrepareDto(yearly);
            }

            if (allTime is not null)
            {
                dto.AllTime = await PrepareDto(allTime);
            }

            return dto;
        }
    }
}
