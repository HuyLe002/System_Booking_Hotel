namespace HotelBookingAPI.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int Nights => (CheckOutDate.Date - CheckInDate.Date).Days;

    public List<BookingServiceDto> Services { get; set; } = new();
    public decimal ServicesTotal { get; set; }
    public decimal RoomCharge => TotalPrice + DiscountAmount - ServicesTotal;

    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }

    public decimal TotalPrice { get; set; }

    // Thanh toán (tính từ sổ giao dịch)
    public decimal TotalPaid { get; set; }
    public decimal TotalRefunded { get; set; }
    public decimal NetPaid => TotalPaid - TotalRefunded;
    public decimal BalanceDue { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
}