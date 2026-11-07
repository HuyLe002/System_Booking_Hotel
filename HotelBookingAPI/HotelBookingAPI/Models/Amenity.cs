namespace HotelBookingAPI.Models
{
    public class Amenity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
