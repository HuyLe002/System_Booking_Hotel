namespace HotelBookingAPI.Common;

public class BookingFilter
{
    public string? Status { get; set; }
    public int? CustomerId { get; set; }
    public int? RoomId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}