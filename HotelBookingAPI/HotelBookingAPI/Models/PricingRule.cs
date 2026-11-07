namespace HotelBookingAPI.Models;

public class PricingRule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RuleType { get; set; } = "Weekend";   // Weekend / Season
    public DateTime? StartDate { get; set; }            // chỉ dùng cho Season
    public DateTime? EndDate { get; set; }              // chỉ dùng cho Season
    public int? RoomTypeId { get; set; }                // null = áp cho mọi loại phòng
    public decimal Percent { get; set; }                // +20 = tăng 20%, -10 = giảm 10%
    public int Priority { get; set; }                   // số lớn thắng
    public bool IsActive { get; set; } = true;

    public RoomType? RoomType { get; set; }
}