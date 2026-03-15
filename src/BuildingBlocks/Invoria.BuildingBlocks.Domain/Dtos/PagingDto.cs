namespace Invoria.BuildingBlocks.Domain.Dtos
{
    public class PagingDto<T>
    {
        public IEnumerable<T> Data { get; set; }
        public PagingInfoDto Info { get; set; }
    }
}
