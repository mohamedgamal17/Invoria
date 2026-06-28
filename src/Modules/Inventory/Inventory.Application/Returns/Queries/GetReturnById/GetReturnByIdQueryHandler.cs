using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Returns.Factories;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Returns.Queries.GetReturnById;

public class GetReturnByIdQueryHandler
    : IApplicatonRequestHandler<GetReturnByIdQuery, ReturnDto>
{
    private readonly IInventoryRepository<Return> _returnRepository;
    private readonly IReturnResponseFactory _returnResponseFactory;

    public GetReturnByIdQueryHandler(
        IInventoryRepository<Return> returnRepository,
        IReturnResponseFactory returnResponseFactory)
    {
        _returnRepository = returnRepository;
        _returnResponseFactory = returnResponseFactory;
    }

    public async Task<Result<ReturnDto>> Handle(
        GetReturnByIdQuery request, CancellationToken cancellationToken)
    {
        var returnEntity = await _returnRepository.SingleOrDefault(
            x => x.Id == request.Id, cancellationToken);

        if (returnEntity is null)
        {
            return Result.Failure<ReturnDto>(
                new NotFoundException($"Return with ID {request.Id} not found"));
        }

        var dto = await _returnResponseFactory.PrepareDto(returnEntity);
        return dto;
    }
}
