namespace HotelBookingAPI.Models;

// Một dòng trong sổ giao dịch: chỉ thêm mới, không sửa, không xóa
public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }                 // luôn dương, chiều thu hay hoàn do Type quyết định
    public string Type { get; set; } = "Payment";       // Payment (thu) / Refund (hoàn)
    public string Method { get; set; } = "Cash";        // Cash / BankTransfer / Card / Original (hoàn về phương thức ban đầu)
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public Booking? Booking { get; set; }
}