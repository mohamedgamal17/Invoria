using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Catalog.Contracts.Dtos;

namespace Invoria.Catalog.Application.ReportProductMetrics.Queries.ListProductMetrics
{
    public class ListProductMetricsQuery : PagingParams, IQuery<PagingDto<ReportProductMetricsPeriodDto>>
    {
        public ReportPeriod Period { get; set; }
    }
}
