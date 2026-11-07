using System.ComponentModel.DataAnnotations;
namespace HotelBookingAPI.DTOs
{
    public class CreateRoomTypeDto
    {
        [Required(ErrorMessage = "Tên loại phòng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string? Description { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal PricePerNight { get; set; }

        [Range(1, 20, ErrorMessage = "Sức chứa từ 1 đến 20 người")]
        public int Capacity { get; set; }
    }
}
