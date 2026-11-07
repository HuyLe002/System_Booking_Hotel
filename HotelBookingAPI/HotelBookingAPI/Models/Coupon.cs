namespace HotelBookingAPI.Models;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;          // luôn lưu chữ HOA
    public string DiscountType { get; set; } = "Percent";    // Percent / Fixed
    public decimal Value { get; set; }                        // Percent: 1-100, Fixed: số tiền
    public decimal? MaxDiscount { get; set; }                 // trần giảm tối đa (chỉ cho Percent)
    public decimal MinBookingAmount { get; set; }             // tạm tính tối thiểu để dùng mã
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? MaxUses { get; set; }                         // null = không giới hạn
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;

    // Concurrency token: SQL Server tự đổi giá trị mỗi lần dòng này bị UPDATE
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}