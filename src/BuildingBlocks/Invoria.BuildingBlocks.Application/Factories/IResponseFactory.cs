using Invoria.BuildingBlocks.Application.Abstractions.Responses;
namespace Invoria.BuildingBlocks.Application.Factories
{
    public interface IResponseFactory<TView, TDto>

    {
        Task<PagedResult<TDto>> PreparePagingDto(PagedResult<TView> paging);
        Task<List<TDto>> PrepareListDto(List<TView> views);
        Task<TDto> PrepareDto(TView view);
    }
}
