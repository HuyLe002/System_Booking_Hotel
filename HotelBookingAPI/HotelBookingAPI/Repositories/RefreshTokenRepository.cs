using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly HotelDbContext _context;

    public RefreshTokenRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash) =>
        await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

    public async Task AddAsync(RefreshToken token) =>
        await _context.RefreshTokens.AddAsync(token);

    // ExecuteUpdate chạy một câu UPDATE duy nhất ngay lập tức, không cần nạp từng dòng
    public async Task RevokeAllForUserAsync(int userId) =>
        await _context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, (DateTime?)DateTime.UtcNow));

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}