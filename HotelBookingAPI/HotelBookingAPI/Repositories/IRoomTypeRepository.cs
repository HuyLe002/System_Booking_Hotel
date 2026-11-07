using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface IRoomTypeRepository
{
    Task<List<RoomType>> GetAllAsync();
    Task<RoomType?> GetByIdAsync(int id);          // chỉ đọc (AsNoTracking)
    Task<RoomType?> GetForUpdateAsync(int id);     // có tracking, dùng khi sửa hoặc xóa
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<bool> HasRoomsAsync(int id);
    Task AddAsync(RoomType entity);
    void Remove(RoomType entity);
    Task SaveChangesAsync();
}