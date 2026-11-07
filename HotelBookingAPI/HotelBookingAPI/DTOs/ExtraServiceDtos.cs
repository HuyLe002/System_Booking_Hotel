using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs;

public class ExtraServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateExtraServiceDto
{
    [Required(ErrorMessage = "Tên dịch vụ là bắt buộc")]
    [MaxLength(100, ErrorMessage = "Tên dịch vụ tối đa 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100000000, ErrorMessage = "Giá phải từ 1 đến 100.000.000")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
    [MaxLength(30, ErrorMessage = "Đơn vị tính tối đa 30 ký tự")]
    public string Unit { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class UpdateExtraServiceDto
{
    [Required(ErrorMessage = "Tên dịch vụ là bắt buộc")]
    [MaxLength(100, ErrorMessage = "Tên dịch vụ tối đa 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100000000, ErrorMessage = "Giá phải từ 1 đến 100.000.000")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
    [MaxLength(30, ErrorMessage = "Đơn vị tính tối đa 30 ký tự")]
    public string Unit { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

// Một dòng dịch vụ client gửi lên khi đặt phòng hoặc đổi dịch vụ
public class BookingServiceItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "ExtraServiceId không hợp lệ")]
    public int ExtraServiceId { get; set; }

    [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
    public int Quantity { get; set; }
}

public class UpdateBookingServicesDto
{
    // Danh sách rỗng nghĩa là bỏ hết dịch vụ
    public List<BookingServiceItemDto> Services { get; set; } = new();
}

// Một dòng dịch vụ trả về trong BookingDto
public class BookingServiceDto
{
    public int ExtraServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}