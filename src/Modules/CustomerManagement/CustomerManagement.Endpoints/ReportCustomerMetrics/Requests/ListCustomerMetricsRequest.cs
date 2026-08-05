using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.CustomerManagement.Endpoints.ReportCustomerMetrics.Requests
{
    public class ListCustomerMetricsRequest : PagingParams
    {
        [QueryParam]
        public ReportPeriod Period { get; set; }
    }

    public class ListCustomerMetricsRequestValidator : AbstractValidator<ListCustomerMetricsRequest>
    {
        public ListCustomerMetricsRequestValidator()
        {
            Include(new PagingParamasValidator<ListCustomerMetricsRequest>());

            RuleFor(x => x.Period)
                .IsInEnum();
        }
    }
}
