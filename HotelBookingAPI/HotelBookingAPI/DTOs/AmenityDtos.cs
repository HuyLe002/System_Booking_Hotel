using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs;

public class AmenityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
}

public class CreateAmenityDto
{
    [Required(ErrorMessage = "Tên tiện nghi là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
    public string Name { get; set; } = string.Empty;
}

public class UpdateAmenityDto
{
    [Required(ErrorMessage = "Tên tiện nghi là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
    public string Name { get; set; } = string.Empty;
}