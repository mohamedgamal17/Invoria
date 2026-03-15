using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.BuildingBlocks.Application.Abstractions.Cqrs;

public interface ICommand<T> : IApplicationRequest<T>
{

}

public interface ICommand : ICommand<Empty> { }


