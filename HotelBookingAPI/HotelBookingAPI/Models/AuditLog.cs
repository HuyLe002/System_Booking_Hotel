using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Models;

// Chỉ thêm, không sửa, không xóa
[Index(nameof(EntityType), nameof(EntityId))]
public class AuditLog
{
    public int Id { get; set; }

    // Giờ UTC
    public DateTime CreatedAt { get; set; }

    // Người thực hiện (chụp tại thời điểm làm)
    public int? UserId { get; set; }

    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    [MaxLength(500)]
    public string? Details { get; set; }
}