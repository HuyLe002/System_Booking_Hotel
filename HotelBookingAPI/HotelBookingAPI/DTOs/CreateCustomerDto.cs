using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs;

public class CreateCustomerDto
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^(0|\+84)\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 0900000001)")]
    public string? Phone { get; set; }
}