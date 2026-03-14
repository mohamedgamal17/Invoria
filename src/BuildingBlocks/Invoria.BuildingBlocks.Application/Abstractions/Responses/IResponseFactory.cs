namespace Invoria.BuildingBlocks.Application.Abstractions.Responses;

public interface IResponseFactory<in TDomain, out TResponse>
{
    TResponse Create(TDomain domainObject);
}

