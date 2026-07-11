using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Returns.Factories;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Contracts.Returns.Enums;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Returns;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Returns.Commands.ApproveReturn;

public sealed class ApproveReturnCommandHandler
    : IApplicatonRequestHandler<ApproveReturnCommand, ReturnDto>
{
    private readonly IInventoryRepository<Return> _returnRepository;
    private readonly IReturnResponseFactory _returnResponseFactory;

    public ApproveReturnCommandHandler(
        IInventoryRepository<Return> returnRepository,
        IReturnResponseFactory returnResponseFactory)
    {
        _returnRepository = returnRepository;
        _returnResponseFactory = returnResponseFactory;
    }

    public async Task<Result<ReturnDto>> Handle(
        ApproveReturnCommand request,
        CancellationToken cancellationToken)
    {
        var @return = await _returnRepository
            .AsQuerable()
            .SingleOrDefaultAsync(r => r.Id == request.ReturnId, cancellationToken);

        if (@return == null)
        {
            return Result.Failure<ReturnDto>(
                new NotFoundException($"Return with ID {request.ReturnId} not found"));
        }

        if (@return.Status != ReturnStatus.Pending)
        {
            return Result.Failure<ReturnDto>(new BusinessLogicException(
                "Return can only be approved when it is Pending."));
        }

        @return.Approve();

        await _returnRepository.Update(@return, cancellationToken);

        var dto = await _returnResponseFactory.PrepareDto(@return);

        return Result.Success(dto);
    }
}
