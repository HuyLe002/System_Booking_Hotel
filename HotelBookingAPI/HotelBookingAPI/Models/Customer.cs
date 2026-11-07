namespace HotelBookingAPI.Models;

public class Customer : ISoftDelete
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // Xóa mềm
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}