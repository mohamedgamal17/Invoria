using Invoria.BuildingBlocks.Application.Factories;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.ReportCustomerMetrics.Factories
{
    using ReportCustomerMetricsEntity = Invoria.CustomerManagement.Domain.Customers.ReportCustomerMetrics;

    public interface IReportCustomerMetricsResponseFactory : IResponseFactory<ReportCustomerMetricsEntity, ReportCustomerMetricsPeriodDto>
    {
        Task<ReportCustomerMetricsDto> PrepareMetricsDto(ReportCustomerMetricsEntity report);

        Task<ReportCustomerMetricsDto> PrepareMetricsDto(
            ReportCustomerMetricsEntity? daily,
            ReportCustomerMetricsEntity? monthly,
            ReportCustomerMetricsEntity? yearly,
            ReportCustomerMetricsEntity? allTime);
    }
}
