using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderInvoice;

public sealed class RecordOrderInvoiceCommandHandler
    : IApplicatonRequestHandler<RecordOrderInvoiceCommand, Empty>
{
    private readonly IOrderingRepository<Order> _orderRepository;

    public RecordOrderInvoiceCommandHandler(IOrderingRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<Empty>> Handle(
        RecordOrderInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .AsQuerable()
            .SingleAsync(o => o.Id == request.OrderId, cancellationToken);

        order.RecordInvoice(request.InvoiceId);

        await _orderRepository.Update(order, cancellationToken);

        return Result.Success(Empty.Value);
    }
}
