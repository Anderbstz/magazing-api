namespace Magazing.Domain.Common;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        return new PagedResult<TResult>
        {
            Items = Items.Select(selector).ToList(),
            Page = Page,
            PageSize = PageSize,
            TotalCount = TotalCount
        };
    }
}
