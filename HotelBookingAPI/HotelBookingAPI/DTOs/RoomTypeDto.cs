namespace HotelBookingAPI.DTOs
{
    public class RoomTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
    }
}
