using Invoria.BuildingBlocks.Application.Abstractions.Responses;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Entities;
namespace Invoria.BuildingBlocks.Application.Factories
{
    public abstract class ResponseFactory<TView, TDto> : IResponseFactory<TView, TDto>
        where TView : class
        where TDto : class
    {
        public virtual async Task<PagedResult<TDto>> PreparePagingDto(PagedResult<TView> paging)
        {
            var data = await PrepareListDto(paging.Items.ToList());


            var pagedDto = new PagedResult<TDto>(data, paging.Info);
        

            return pagedDto;
        }
        public virtual async Task<List<TDto>> PrepareListDto(List<TView> views)
        {
            var tasks = views.Select(PrepareDto);

            return (await Task.WhenAll(tasks)).ToList();
        }

        public abstract Task<TDto> PrepareDto(TView view);

        protected virtual void MapAudited(IAuditedEntity view , AuditedEntityDto dto)
        {
            dto.CreatedAt = view.CreatedAt;
            dto.CreatedBy = view.CreatedBy;
            dto.LastModifiedAt = view.LastModifiedAt;
            dto.LastModifiedBy = view.LastModifiedBy;

        }
    }
}
