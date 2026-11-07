namespace HotelBookingAPI.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? Details { get; set; }
}