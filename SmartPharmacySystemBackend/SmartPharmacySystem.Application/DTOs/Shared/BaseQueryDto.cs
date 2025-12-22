namespace SmartPharmacySystem.Application.DTOs.Shared;

public class BaseQueryDto
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "Id"; // Default sort column
    public string SortDirection { get; set; } = "asc"; // asc or desc
}
