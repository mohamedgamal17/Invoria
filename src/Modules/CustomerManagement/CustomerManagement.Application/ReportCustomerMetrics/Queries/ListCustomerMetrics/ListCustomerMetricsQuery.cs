using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.CustomerManagement.Contracts.Dtos;

namespace Invoria.CustomerManagement.Application.ReportCustomerMetrics.Queries.ListCustomerMetrics
{
    public class ListCustomerMetricsQuery : PagingParams, IQuery<PagingDto<ReportCustomerMetricsPeriodDto>>
    {
        public ReportPeriod Period { get; set; }
    }
}
