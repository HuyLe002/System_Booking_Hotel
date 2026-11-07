using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface IPricingRuleRepository
{
    Task<List<PricingRule>> GetAllAsync(bool activeOnly);
    Task<PricingRule?> GetByIdAsync(int id);                 // chỉ đọc, kèm RoomType
    Task<PricingRule?> GetForUpdateAsync(int id);            // có tracking
    // Quy tắc đang bật có thể ảnh hưởng một kỳ nghỉ của loại phòng này
    Task<List<PricingRule>> GetApplicableAsync(int roomTypeId, DateTime checkIn, DateTime lastNight);
    Task<Room?> GetRoomWithTypeAsync(int roomId);
    Task<bool> RoomTypeExistsAsync(int roomTypeId);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task AddAsync(PricingRule entity);
    void Remove(PricingRule entity);
    Task SaveChangesAsync();
}