namespace UserManagementService.Models.DTOs;

/// <summary>
/// Pagination metadata for list responses.
/// </summary>
public class Pagination
{
    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int ItemsPerPage { get; set; }

    /// <summary>
    /// Indicates if there is a next page.
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>
    /// Indicates if there is a previous page.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Creates pagination metadata from query parameters.
    /// </summary>
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
/// Paginated response wrapper.
/// </summary>
/// <typeparam name="T">Type of items in the response.</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// List of items for the current page.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Pagination metadata.
    /// </summary>
    public Pagination Pagination { get; set; } = new();
}

/// <summary>
/// Query parameters for pagination.
/// </summary>
public class PaginationQuery
{
    private int _page = 1;
    private int _pageSize = 10;

    /// <summary>
    /// Page number (1-based, defaults to 1).
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Number of items per page (defaults to 10, max 100).
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
    }

    /// <summary>
    /// Number of items to skip.
    /// </summary>
    public int Skip => (Page - 1) * PageSize;
}
