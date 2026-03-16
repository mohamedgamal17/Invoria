using Invoria.BuildingBlocks.Application.Abstractions.Responses;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Entities;
namespace Invoria.BuildingBlocks.Application.Factories
{
    public abstract class ResponseFactory<TView, TDto> : IResponseFactory<TView, TDto>
        where TView : class
        where TDto : class
    {
        public virtual async Task<PagingDto<TDto>> PreparePagingDto(PagingDto<TView> paging)
        {
            var data = await PrepareListDto(paging.Data.ToList());


            var pagedDto = new PagingDto<TDto>
            {
                Data = data,
                Info = paging.Info
            };
        

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
