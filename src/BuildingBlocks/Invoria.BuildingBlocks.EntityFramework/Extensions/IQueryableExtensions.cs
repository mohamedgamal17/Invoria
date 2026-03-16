using Invoria.BuildingBlocks.Domain.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Invoria.BuildingBlocks.EntityFramework.Extensions
{
    public static class IQueryableExtensions
    {
        public static async Task<PagingDto<T>> ToPaged<T>(this IQueryable<T> query, int skip, int length)
        {
            var result = await query.Skip(skip).Take(length).ToListAsync();

            var count = await query.CountAsync();

            var info = new PagingInfoDto()
            {
                Skip = skip,
                Length = length,
                TotalCount = count
            };


            var paging = new PagingDto<T>()
            {
                Data = result,
                Info = info
            };
            return paging;
        }

    }
}
