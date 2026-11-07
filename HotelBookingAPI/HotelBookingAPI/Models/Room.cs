namespace HotelBookingAPI.Models;

public class Room : ISoftDelete
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Available"; // Available / Maintenance
    public int RoomTypeId { get; set; }

    public int Floor { get; set; } = 1;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    // Xóa mềm
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public RoomType? RoomType { get; set; }
    public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}