namespace CourseLibrary.API.ResourceParameters;

public class AuthorResourceParameters
{
    private const int _maxPageSize = 20;
    private int _pageSize = 10;
    public string? MainCategory { get; set; }
    public string? SearchQuery { get; set; }
    public string OrderBy { get; set; } = "Name";
    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Min(value, _maxPageSize);
    }
}