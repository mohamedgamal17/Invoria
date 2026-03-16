using Invoria.BuildingBlocks.Application.Abstractions.Responses;
using Invoria.BuildingBlocks.Domain.Dtos;
namespace Invoria.BuildingBlocks.Application.Factories
{
    public interface IResponseFactory<TView, TDto>

    {
        Task<PagingDto<TDto>> PreparePagingDto(PagingDto<TView> paging);
        Task<List<TDto>> PrepareListDto(List<TView> views);
        Task<TDto> PrepareDto(TView view);
    }
}
