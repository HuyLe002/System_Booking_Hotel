using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs;

public class UpdateBookingDatesDto
{
    [Required(ErrorMessage = "Ngày nhận phòng là bắt buộc")]
    public DateTime CheckInDate { get; set; }

    [Required(ErrorMessage = "Ngày trả phòng là bắt buộc")]
    public DateTime CheckOutDate { get; set; }
}