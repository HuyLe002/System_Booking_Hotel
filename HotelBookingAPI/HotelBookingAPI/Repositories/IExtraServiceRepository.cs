using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface IExtraServiceRepository
{
    Task<List<ExtraService>> GetAllAsync(bool activeOnly);
    Task<ExtraService?> GetByIdAsync(int id);          // chỉ đọc
    Task<ExtraService?> GetForUpdateAsync(int id);     // có tracking
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<bool> IsUsedAsync(int id);
    Task AddAsync(ExtraService entity);
    void Remove(ExtraService entity);
    Task SaveChangesAsync();
}