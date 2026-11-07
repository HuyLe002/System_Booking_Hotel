using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
}

public class CreateStaffDto
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự")]
    [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Tên đăng nhập chỉ gồm chữ, số, dấu chấm, gạch ngang, gạch dưới")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu từ 8 đến 100 ký tự")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Mật khẩu phải có cả chữ và số")]
    public string Password { get; set; } = string.Empty;
}