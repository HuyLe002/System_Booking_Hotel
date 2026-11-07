using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs;

public class CreateBookingDto
{
    // Admin: bắt buộc nhập khách hàng cần đặt giúp
    // Customer: bỏ qua, hệ thống tự lấy từ token
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "RoomId không hợp lệ")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Ngày nhận phòng là bắt buộc")]
    public DateTime CheckInDate { get; set; }

    [Required(ErrorMessage = "Ngày trả phòng là bắt buộc")]
    public DateTime CheckOutDate { get; set; }

    // Không bắt buộc: có thể đặt phòng không kèm dịch vụ
    public List<BookingServiceItemDto>? Services { get; set; }

    // Không bắt buộc: mã giảm giá
    [MaxLength(20, ErrorMessage = "Mã giảm giá tối đa 20 ký tự")]
    public string? CouponCode { get; set; }
}