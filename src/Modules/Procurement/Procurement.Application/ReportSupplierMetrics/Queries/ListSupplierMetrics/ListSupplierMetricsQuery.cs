using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.ReportSupplierMetrics.Queries.ListSupplierMetrics;

public class ListSupplierMetricsQuery : PagingParams, IQuery<PagingDto<ReportSupplierMetricsPeriodDto>>
{
    public ReportPeriod Period { get; set; }
}