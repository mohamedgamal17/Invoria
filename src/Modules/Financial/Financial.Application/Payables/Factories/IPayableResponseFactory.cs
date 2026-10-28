using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Domain.Payables;

namespace Invoria.Financial.Application.Payables.Factories;

public interface IPayableResponseFactory : IResponseFactory<Payable, PayableDto>
{
}
