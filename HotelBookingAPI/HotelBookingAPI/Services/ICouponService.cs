using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface ICouponService
{
    Task<List<CouponDto>> GetAllAsync(bool activeOnly);
    Task<CouponDto> GetByIdAsync(int id);
    Task<CouponDto> CreateAsync(CreateCouponDto dto);
    Task UpdateAsync(int id, UpdateCouponDto dto);
    Task DeleteAsync(int id);
}