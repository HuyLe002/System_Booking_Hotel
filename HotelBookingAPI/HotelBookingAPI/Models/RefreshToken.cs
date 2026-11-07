namespace HotelBookingAPI.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;   // chỉ lưu băm, không lưu token gốc
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }                // null = còn hiệu lực

    public User? User { get; set; }
}