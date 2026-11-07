using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash);   // có tracking, kèm User
    Task AddAsync(RefreshToken token);
    Task RevokeAllForUserAsync(int userId);                  // cập nhật ngay, không cần SaveChanges
    Task SaveChangesAsync();
}