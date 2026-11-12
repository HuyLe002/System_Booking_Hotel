namespace HotelBookingAPI.Models;

public class Booking
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }

    // Bất biến: TotalPrice = (tiền phòng + tiền dịch vụ) - DiscountAmount
    public decimal TotalPrice { get; set; }
    public int? CouponId { get; set; }
    public decimal DiscountAmount { get; set; }

    // Pending / Confirmed / CheckedIn / CheckedOut / Cancelled (xem BookingStatuses)
    public string Status { get; set; } = "Pending";

    // Thời điểm thực tế khách nhận và trả phòng (giờ UTC)
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }

    // Token chống ghi đè đồng thời, được ứng dụng đổi mỗi lần cập nhật
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Customer? Customer { get; set; }
    public Room? Room { get; set; }
    public Coupon? Coupon { get; set; }
    public ICollection<BookingExtraService> ExtraServices { get; set; } = new List<BookingExtraService>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<PaymentClaim> PaymentClaims { get; set; } = new List<PaymentClaim>();
}