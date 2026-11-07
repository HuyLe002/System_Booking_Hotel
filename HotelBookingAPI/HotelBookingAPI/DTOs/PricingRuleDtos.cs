using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs;

public class PricingRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? RoomTypeId { get; set; }
    public string? RoomTypeName { get; set; }
    public decimal Percent { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
}

// Các trường dùng chung cho tạo và sửa
public abstract class PricingRuleInputDto
{
    [Required(ErrorMessage = "Tên quy tắc là bắt buộc")]
    [MaxLength(100, ErrorMessage = "Tên quy tắc tối đa 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loại quy tắc là bắt buộc")]
    [RegularExpression("^(Weekend|Season)$", ErrorMessage = "RuleType chỉ nhận: Weekend hoặc Season")]
    public string RuleType { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Bỏ trống = áp dụng cho mọi loại phòng
    public int? RoomTypeId { get; set; }

    [Range(-90, 300, ErrorMessage = "Phần trăm điều chỉnh phải từ -90 đến 300")]
    public decimal Percent { get; set; }

    [Range(0, 100, ErrorMessage = "Độ ưu tiên phải từ 0 đến 100")]
    public int Priority { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CreatePricingRuleDto : PricingRuleInputDto
{
}

public class UpdatePricingRuleDto : PricingRuleInputDto
{
}

// Báo giá một đêm
public class QuoteNightDto
{
    public DateTime Date { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal Price { get; set; }
    public string? RuleName { get; set; }
    public decimal? Percent { get; set; }
}

// Báo giá cả kỳ nghỉ
public class QuoteDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public int Nights { get; set; }
    public decimal BasePricePerNight { get; set; }
    public decimal RoomCharge { get; set; }
    public decimal DifferenceFromBase => RoomCharge - BasePricePerNight * Nights;
    public List<QuoteNightDto> Items { get; set; } = new();
}