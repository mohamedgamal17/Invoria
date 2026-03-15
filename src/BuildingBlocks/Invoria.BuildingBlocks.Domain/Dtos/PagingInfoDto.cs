namespace Invoria.BuildingBlocks.Domain.Dtos
{
    public class PagingInfoDto
    {
        public int Length { get; set; }
        public int Skip { get; set; }
        public long TotalCount { get; set; }
    }
}
