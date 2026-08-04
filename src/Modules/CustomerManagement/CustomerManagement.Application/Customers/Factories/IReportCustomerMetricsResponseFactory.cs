using Invoria.BuildingBlocks.Application.Factories;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.Customers.Factories
{
    public interface IReportCustomerMetricsResponseFactory : IResponseFactory<ReportCustomerMetrics, ReportCustomerMetricsPeriodDto>
    {
        Task<ReportCustomerMetricsDto> PrepareMetricsDto(ReportCustomerMetrics report);
    }
}
