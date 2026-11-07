using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs;

public class CouponDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? MaxDiscount { get; set; }
    public decimal MinBookingAmount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public int? RemainingUses => MaxUses == null ? null : MaxUses - UsedCount;
    public bool IsActive { get; set; }
}

// Các trường dùng chung cho tạo và sửa (Code không được sửa sau khi tạo)
public abstract class CouponInputDto
{
    [Required(ErrorMessage = "Loại giảm giá là bắt buộc")]
    [RegularExpression("^(Percent|Fixed)$", ErrorMessage = "DiscountType chỉ nhận: Percent hoặc Fixed")]
    public string DiscountType { get; set; } = string.Empty;

    [Range(1, 100000000, ErrorMessage = "Giá trị giảm phải từ 1 đến 100.000.000")]
    public decimal Value { get; set; }

    [Range(1, 100000000, ErrorMessage = "Mức giảm tối đa phải từ 1 đến 100.000.000")]
    public decimal? MaxDiscount { get; set; }

    [Range(0, 100000000, ErrorMessage = "Đơn tối thiểu phải từ 0 đến 100.000.000")]
    public decimal MinBookingAmount { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
    public DateTime ValidFrom { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
    public DateTime ValidTo { get; set; }

    [Range(1, 1000000, ErrorMessage = "Số lượt dùng tối đa phải từ 1 đến 1.000.000")]
    public int? MaxUses { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CreateCouponDto : CouponInputDto
{
    [Required(ErrorMessage = "Mã giảm giá là bắt buộc")]
    [RegularExpression(@"^[A-Za-z0-9_-]{3,20}$", ErrorMessage = "Mã gồm 3-20 ký tự: chữ, số, gạch ngang hoặc gạch dưới")]
    public string Code { get; set; } = string.Empty;
}

public class UpdateCouponDto : CouponInputDto
{
}