using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class PricingRuleRepository : IPricingRuleRepository
{
    private readonly HotelDbContext _context;

    public PricingRuleRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<PricingRule>> GetAllAsync(bool activeOnly)
    {
        var query = _context.PricingRules.AsNoTracking().Include(x => x.RoomType).AsQueryable();

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<PricingRule?> GetByIdAsync(int id) =>
        await _context.PricingRules
            .AsNoTracking()
            .Include(x => x.RoomType)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PricingRule?> GetForUpdateAsync(int id) =>
        await _context.PricingRules.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<PricingRule>> GetApplicableAsync(int roomTypeId, DateTime checkIn, DateTime lastNight) =>
        await _context.PricingRules
            .AsNoTracking()
            .Where(r => r.IsActive
                && (r.RoomTypeId == null || r.RoomTypeId == roomTypeId)
                // Weekend luôn có thể trúng, Season chỉ lấy loại giao với kỳ nghỉ
                && (r.RuleType == "Weekend" || (r.StartDate <= lastNight && r.EndDate >= checkIn)))
            .ToListAsync();

    public async Task<Room?> GetRoomWithTypeAsync(int roomId) =>
        await _context.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .FirstOrDefaultAsync(x => x.Id == roomId);

    public async Task<bool> RoomTypeExistsAsync(int roomTypeId) =>
        await _context.RoomTypes.AnyAsync(x => x.Id == roomTypeId);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
        await _context.PricingRules.AnyAsync(x =>
            x.Name == name && (excludeId == null || x.Id != excludeId));

    public async Task AddAsync(PricingRule entity) =>
        await _context.PricingRules.AddAsync(entity);

    public void Remove(PricingRule entity) =>
        _context.PricingRules.Remove(entity);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}