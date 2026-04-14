namespace UserManagementService.Models;

/// <summary>
/// Pagination metadata for list responses.
/// </summary>
public class Pagination
{
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int ItemsPerPage { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public static Pagination Create(int totalItems, int currentPage, int itemsPerPage)
    {
        var totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);
        return new Pagination
        {
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            ItemsPerPage = itemsPerPage
        };
    }
}

/// <summary>
/// Query parameters for paginated requests.
/// </summary>
public class PaginationQuery
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
