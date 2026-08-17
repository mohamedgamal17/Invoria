using Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Commands.RecordOrderSalesProfitMetrics;
using Invoria.Ordering.Domain.OrderAllocationConsumptions.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Invoria.Ordering.Application.OrderAllocationConsumptions.Handlers;

public sealed class OrderAllocationConsumptionCreatedDomainEventHandler
    : INotificationHandler<OrderAllocationConsumptionCreatedDomainEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OrderAllocationConsumptionCreatedDomainEventHandler> _logger;

    public OrderAllocationConsumptionCreatedDomainEventHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OrderAllocationConsumptionCreatedDomainEventHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task Handle(
        OrderAllocationConsumptionCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Recording order sales profit for OrderId={OrderId} AllocationId={AllocationId}",
            notification.OrderId,
            notification.AllocationId);

        using var scope = _serviceScopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(
            new RecordOrderSalesProfitMetricsCommand
            {
                OrderId = notification.OrderId,
                AllocationId = notification.AllocationId,
                OccurredOn = notification.OccurredOn
            },
            cancellationToken);

        if (result.IsFailure)
        {
            throw result.Exception!;
        }
    }
}