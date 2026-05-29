using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Fulfillments;
using Invoria.Inventory.Domain.Fulfillments.Services;

namespace Invoria.Inventory.Application.Fulfillments.Commands.CreateFulfillment;

public sealed class CreateFulfillmentCommandHandler
    : IApplicatonRequestHandler<CreateFulfillmentCommand, Empty>
{
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IInventoryRepository<Allocation> _allocationRepository;
    private readonly IInventoryRepository<Fulfillment> _fulfillmentRepository;
    private readonly IFulfillmentDomainService _fulfillmentDomainService;

    public CreateFulfillmentCommandHandler(
        IInventoryUnitOfWork unitOfWork,
        IInventoryRepository<Allocation> allocationRepository,
        IInventoryRepository<Fulfillment> fulfillmentRepository,
        IFulfillmentDomainService fulfillmentDomainService)
    {
        _unitOfWork = unitOfWork;
        _allocationRepository = allocationRepository;
        _fulfillmentRepository = fulfillmentRepository;
        _fulfillmentDomainService = fulfillmentDomainService;
    }

    public async Task<Result<Empty>> Handle(
        CreateFulfillmentCommand request,
        CancellationToken cancellationToken)
    {
        var allocationId = request.AllocationId;

        var allocation = await _allocationRepository.SingleOrDefault(
            a => a.Id == allocationId,
            cancellationToken);

        if (allocation is null)
        {
            return Result.Failure<Empty>(
                new NotFoundException($"Allocation with ID {allocationId} not found"));
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var createResult = _fulfillmentDomainService.CreateFulfillment(allocation);
            if (createResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<Empty>(createResult.Exception!);
            }

            await _fulfillmentRepository.Add(createResult.Value!, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(Empty.Value);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<Empty>(
                new InvalidOperationException($"Create fulfillment failed: {ex.Message}", ex));
        }
    }
}
