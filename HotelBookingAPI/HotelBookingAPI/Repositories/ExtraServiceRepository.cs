using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class ExtraServiceRepository : IExtraServiceRepository
{
    private readonly HotelDbContext _context;

    public ExtraServiceRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExtraService>> GetAllAsync(bool activeOnly)
    {
        var query = _context.ExtraServices.AsNoTracking().AsQueryable();

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query.OrderBy(x => x.Name).ThenBy(x => x.Id).ToListAsync();
    }

    public async Task<ExtraService?> GetByIdAsync(int id) =>
        await _context.ExtraServices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<ExtraService?> GetForUpdateAsync(int id) =>
        await _context.ExtraServices.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
        await _context.ExtraServices.AnyAsync(x =>
            x.Name == name && (excludeId == null || x.Id != excludeId));

    public async Task<bool> IsUsedAsync(int id) =>
        await _context.BookingExtraServices.AnyAsync(x => x.ExtraServiceId == id);

    public async Task AddAsync(ExtraService entity) =>
        await _context.ExtraServices.AddAsync(entity);

    public void Remove(ExtraService entity) =>
        _context.ExtraServices.Remove(entity);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}