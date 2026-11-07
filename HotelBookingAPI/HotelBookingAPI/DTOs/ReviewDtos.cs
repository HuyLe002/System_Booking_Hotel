using System.ComponentModel.DataAnnotations;
using HotelBookingAPI.Common;

namespace HotelBookingAPI.DTOs;

public class CreateReviewDto
{
    [Range(1, int.MaxValue, ErrorMessage = "BookingId không hợp lệ")]
    public int BookingId { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
    public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Nội dung tối đa 1000 ký tự")]
    public string? Comment { get; set; }
}

public class UpdateReviewDto
{
    [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
    public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Nội dung tối đa 1000 ký tự")]
    public string? Comment { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Kết quả cho trang đánh giá của một phòng: điểm trung bình + danh sách phân trang
public class RoomReviewsDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public PagedResult<ReviewDto> Reviews { get; set; } = new();
}