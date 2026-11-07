namespace HotelBookingAPI.DTOs;

public class RoomDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int? Nights { get; set; }
    public decimal? QuotedRoomCharge { get; set; }
    public List<QuoteNightDto> QuoteItems { get; set; } = new();
    public int Floor { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Amenities { get; set; } = new();
    public List<int> AmenityIds { get; set; } = new();
}