using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface ICouponRepository
{
    Task<List<Coupon>> GetAllAsync(bool activeOnly);
    Task<Coupon?> GetByIdAsync(int id);              // chỉ đọc
    Task<Coupon?> GetForUpdateAsync(int id);         // có tracking
    Task<bool> CodeExistsAsync(string code);
    Task<bool> IsUsedAsync(int id);
    Task AddAsync(Coupon entity);
    void Remove(Coupon entity);
    Task SaveChangesAsync();
}