using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Fulfillments;

namespace Invoria.Inventory.Application.Fulfillments.Commands.RequestDispatchFulfillment;

public sealed class RequestDispatchFulfillmentCommandHandler
    : IApplicatonRequestHandler<RequestDispatchFulfillmentCommand, Empty>
{
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IInventoryRepository<Fulfillment> _fulfillmentRepository;

    public RequestDispatchFulfillmentCommandHandler(
        IInventoryUnitOfWork unitOfWork,
        IInventoryRepository<Fulfillment> fulfillmentRepository)
    {
        _unitOfWork = unitOfWork;
        _fulfillmentRepository = fulfillmentRepository;
    }

    public async Task<Result<Empty>> Handle(
        RequestDispatchFulfillmentCommand request,
        CancellationToken cancellationToken)
    {
        var fulfillmentId = request.FulfillmentId;

        var fulfillment = await _fulfillmentRepository.SingleOrDefault(
            f => f.Id == fulfillmentId,
            cancellationToken);

        if (fulfillment is null)
        {
            return Result.Failure<Empty>(
                new NotFoundException($"Fulfillment with ID {fulfillmentId} not found"));
        }

        if (fulfillment.Status != FulfillmentStatus.Pending)
        {
            return Result.Failure<Empty>(
                new BusinessLogicException(
                    $"Fulfillment {fulfillmentId} must be in {FulfillmentStatus.Pending} state to request dispatch."));
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            fulfillment.RequestDispatch();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(Empty.Value);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<Empty>(
                new InvalidOperationException($"Request dispatch fulfillment failed: {ex.Message}", ex));
        }
    }
}
