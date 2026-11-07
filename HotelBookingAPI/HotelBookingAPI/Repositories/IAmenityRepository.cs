using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface IAmenityRepository
{
    Task<List<AmenityWithRoomCount>> GetAllAsync(string? keyword);
    Task<AmenityWithRoomCount?> GetByIdWithCountAsync(int id);   // chỉ đọc
    Task<Amenity?> GetForUpdateAsync(int id);                     // có tracking
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<bool> IsUsedByRoomsAsync(int id);
    Task AddAsync(Amenity entity);
    void Remove(Amenity entity);
    Task SaveChangesAsync();
}