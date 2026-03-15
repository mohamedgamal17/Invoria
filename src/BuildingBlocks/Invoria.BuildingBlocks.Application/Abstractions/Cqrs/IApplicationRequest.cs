using Invoria.BuildingBlocks.Domain.Primitives;
using MediatR;

namespace Invoria.BuildingBlocks.Application.Abstractions.Cqrs
{
    public interface IApplicationRequest<T> : IRequest<Result<T>>
    {
    }
}
