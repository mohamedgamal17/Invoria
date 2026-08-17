using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.CustomerManagement.Application.ReportCustomerMetrics.Factories;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.ReportCustomerMetrics.Queries.ListCustomerMetrics
{
    using ReportCustomerMetricsEntity = Invoria.CustomerManagement.Domain.Customers.ReportCustomerMetrics;

    public class ListCustomerMetricsQueryHandler : IApplicatonRequestHandler<ListCustomerMetricsQuery, PagingDto<ReportCustomerMetricsPeriodDto>>
    {
        private readonly ICustomerRepository<ReportCustomerMetricsEntity> _reportCustomerMetricsRepository;
        private readonly IReportCustomerMetricsResponseFactory _reportCustomerMetricsResponseFactory;

        public ListCustomerMetricsQueryHandler(
            ICustomerRepository<ReportCustomerMetricsEntity> reportCustomerMetricsRepository,
            IReportCustomerMetricsResponseFactory reportCustomerMetricsResponseFactory)
        {
            _reportCustomerMetricsRepository = reportCustomerMetricsRepository;
            _reportCustomerMetricsResponseFactory = reportCustomerMetricsResponseFactory;
        }

        public async Task<Result<PagingDto<ReportCustomerMetricsPeriodDto>>> Handle(ListCustomerMetricsQuery request, CancellationToken cancellationToken)
        {
            var query = _reportCustomerMetricsRepository.AsQuerable();

            query = query.Where(x => x.Period == request.Period);

            query = query.OrderByDescending(x => x.Date);

            var result = await query.ToPaged(request.Skip, request.Length);

            var response = await _reportCustomerMetricsResponseFactory.PreparePagingDto(result);

            return response;
        }
    }
}
