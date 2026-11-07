using HotelBookingAPI.Common;
using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface IRoomRepository
{
    Task<(List<Room> Items, int TotalItems)> GetPagedAsync(RoomQuery query, string sortBy, bool desc);
    Task<Room?> GetByIdAsync(int id);          // chỉ đọc, kèm RoomType và Amenities
    Task<Room?> GetForUpdateAsync(int id);     // có tracking, kèm Amenities
    Task<bool> RoomTypeExistsAsync(int roomTypeId);
    Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeId = null);
    Task<List<Amenity>> GetAmenitiesByIdsAsync(List<int> ids);
    Task<bool> HasBookingsAsync(int roomId);
    Task AddAsync(Room entity);
    void Remove(Room entity);
    Task SaveChangesAsync();
}