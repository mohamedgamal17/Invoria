namespace Invoria.BuildingBlocks.Application.Abstractions.Responses;

public class PagedResult<T>
{
 
    public IReadOnlyCollection<T> Items { get; }
    public PagedResultInfo Info { get; set; }
    public PagedResult(IReadOnlyCollection<T> items, PagedResultInfo info)
    {
        Items = items;
        Info = info;
    }
}

public class PagedResultInfo
{
    public int Skip { get; set; }
    public int TotalCount { get; }
    public PagedResultInfo(int skip, int totalCount)
    {
        Skip = skip;
        TotalCount = totalCount;
    }

}

