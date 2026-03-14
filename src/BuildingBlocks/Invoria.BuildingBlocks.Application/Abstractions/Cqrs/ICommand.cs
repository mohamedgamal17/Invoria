namespace Invoria.BuildingBlocks.Application.Abstractions.Cqrs;

public interface ICommand
{
}

public interface ICommand<out TResult>
{
}

