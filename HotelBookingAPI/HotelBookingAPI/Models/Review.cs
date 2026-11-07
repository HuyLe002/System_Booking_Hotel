namespace HotelBookingAPI.Models;

public class Review
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int RoomId { get; set; }
    public int CustomerId { get; set; }
    public int Rating { get; set; }          // 1 - 5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public Booking? Booking { get; set; }
    public Room? Room { get; set; }
    public Customer? Customer { get; set; }
}