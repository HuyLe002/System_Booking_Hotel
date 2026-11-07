using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Common;

public class PagingQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "page phải từ 1 trở lên")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100")]
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }

    [RegularExpression("^(?i)(asc|desc)$", ErrorMessage = "sortDir chỉ nhận asc hoặc desc")]
    public string SortDir { get; set; } = "asc";
}