using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs;

public class CreateRoomDto
{
    [Required(ErrorMessage = "Số phòng là bắt buộc")]
    [StringLength(10, ErrorMessage = "Số phòng tối đa 10 ký tự")]
    public string RoomNumber { get; set; } = string.Empty;

    [RegularExpression("^(Available|Maintenance)$", ErrorMessage = "Status chỉ nhận Available hoặc Maintenance")]
    public string Status { get; set; } = "Available";

    [Range(1, int.MaxValue, ErrorMessage = "RoomTypeId không hợp lệ")]
    public int RoomTypeId { get; set; }

    [Range(1, 50, ErrorMessage = "Tầng phải từ 1 đến 50")]
    public int Floor { get; set; } = 1;

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Đường dẫn ảnh không hợp lệ (phải bắt đầu bằng http:// hoặc https://)")]
    [StringLength(500, ErrorMessage = "Đường dẫn ảnh tối đa 500 ký tự")]
    public string? ImageUrl { get; set; }

    public List<int> AmenityIds { get; set; } = new();
}