using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Contracts.Returns.Enums;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Returns;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Returns.Commands.ApproveReturn;

public sealed class ApproveReturnCommandHandler
    : IApplicatonRequestHandler<ApproveReturnCommand, Empty>
{
    private readonly IInventoryRepository<Return> _returnRepository;

    public ApproveReturnCommandHandler(IInventoryRepository<Return> returnRepository)
    {
        _returnRepository = returnRepository;
    }

    public async Task<Result<Empty>> Handle(
        ApproveReturnCommand request,
        CancellationToken cancellationToken)
    {
        var @return = await _returnRepository
            .AsQuerable()
            .SingleOrDefaultAsync(r => r.Id == request.ReturnId, cancellationToken);

        if (@return == null)
        {
            return Result.Failure<Empty>(
                new NotFoundException($"Return with ID {request.ReturnId} not found"));
        }

        if (@return.Status != ReturnStatus.Pending)
        {
            return Result.Failure<Empty>(new BusinessLogicException(
                "Return can only be approved when it is Pending."));
        }

        @return.Approve();

        await _returnRepository.Update(@return, cancellationToken);

        return Result.Success(Empty.Value);
    }
}
