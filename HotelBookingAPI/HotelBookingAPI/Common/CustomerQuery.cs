namespace HotelBookingAPI.Common;

public class CustomerQuery : PagingQuery
{
    public string? Keyword { get; set; }
}