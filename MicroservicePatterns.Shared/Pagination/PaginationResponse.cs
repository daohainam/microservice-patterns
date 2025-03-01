namespace MicroservicePatterns.Shared.Pagination;

public class PaginatedResult<TEntity>(int index, int pageSize, long count, IEnumerable<TEntity> items) where TEntity : class
{
    public int Index => index;
    public int PageSize => pageSize;
    public long Count => count;
    public IEnumerable<TEntity> Items => items;
}