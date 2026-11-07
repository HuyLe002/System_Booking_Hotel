namespace HotelBookingAPI.Models;

// Bảng trung gian n-n có dữ liệu riêng: số lượng và đơn giá tại thời điểm đặt
public class BookingExtraService
{
    public int BookingId { get; set; }
    public int ExtraServiceId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }   // bản chụp giá, không đổi khi Admin đổi giá sau này

    public Booking? Booking { get; set; }
    public ExtraService? ExtraService { get; set; }
}