using Invoria.BuildingBlocks.Application.Factories;
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

        public async Task<ReportCustomerMetricsDto> PrepareMetricsDto(
            ReportCustomerMetricsEntity daily,
            ReportCustomerMetricsEntity monthly,
            ReportCustomerMetricsEntity yearly,
            ReportCustomerMetricsEntity allTime)
        {
            var dto = new ReportCustomerMetricsDto
            {
                ThisDay = await PrepareDto(daily),
                ThisMonth = await PrepareDto(monthly),
                ThisYear = await PrepareDto(yearly),
                AllTime = await PrepareDto(allTime)
            };

            return dto;
        }
    }
}