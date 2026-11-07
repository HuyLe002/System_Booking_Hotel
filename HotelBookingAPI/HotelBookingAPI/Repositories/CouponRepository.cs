using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly HotelDbContext _context;

    public CouponRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<Coupon>> GetAllAsync(bool activeOnly)
    {
        var query = _context.Coupons.AsNoTracking().AsQueryable();

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query.OrderByDescending(x => x.ValidTo).ThenBy(x => x.Id).ToListAsync();
    }

    public async Task<Coupon?> GetByIdAsync(int id) =>
        await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Coupon?> GetForUpdateAsync(int id) =>
        await _context.Coupons.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<bool> CodeExistsAsync(string code) =>
        await _context.Coupons.AnyAsync(x => x.Code == code);

    public async Task<bool> IsUsedAsync(int id) =>
        await _context.Bookings.AnyAsync(x => x.CouponId == id);

    public async Task AddAsync(Coupon entity) =>
        await _context.Coupons.AddAsync(entity);

    public void Remove(Coupon entity) =>
        _context.Coupons.Remove(entity);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}