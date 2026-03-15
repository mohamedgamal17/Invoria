using Invoria.BuildingBlocks.Domain.Primitives;
using MediatR;

namespace Invoria.BuildingBlocks.Application.Abstractions.Cqrs;


public interface IApplicatonRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : IApplicationRequest<TResponse>
{ 

}



