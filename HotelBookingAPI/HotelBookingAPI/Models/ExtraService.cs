namespace HotelBookingAPI.Models;

public class ExtraService
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Unit { get; set; } = "lần";   // đơn vị tính: suất, lượt, kg...
    public bool IsActive { get; set; } = true;
}