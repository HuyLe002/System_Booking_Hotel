namespace HotelBookingAPI.Common;

public class AuditLogQuery : PagingQuery
{
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? UserName { get; set; }
    public string? Action { get; set; }

    // Lọc theo ngày (giờ UTC), cả hai đầu đều tính
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}