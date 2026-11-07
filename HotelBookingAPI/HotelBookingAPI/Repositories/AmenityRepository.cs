using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class AmenityRepository : IAmenityRepository
{
    private readonly HotelDbContext _context;

    public AmenityRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<AmenityWithRoomCount>> GetAllAsync(string? keyword)
    {
        var query = _context.Amenities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x => x.Name.Contains(keyword));

        // Rooms.Count được EF dịch thành COUNT trong SQL, không tải danh sách phòng lên
        var data = await query
            .OrderBy(x => x.Name)
            .Select(x => new { Amenity = x, TotalRooms = x.Rooms.Count })
            .ToListAsync();

        return data
            .Select(d => new AmenityWithRoomCount(d.Amenity, d.TotalRooms))
            .ToList();
    }

    public async Task<AmenityWithRoomCount?> GetByIdWithCountAsync(int id)
    {
        var data = await _context.Amenities
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { Amenity = x, TotalRooms = x.Rooms.Count })
            .FirstOrDefaultAsync();

        return data == null
            ? null
            : new AmenityWithRoomCount(data.Amenity, data.TotalRooms);
    }

    public async Task<Amenity?> GetForUpdateAsync(int id) =>
        await _context.Amenities.FindAsync(id);

    // excludeId dùng khi cập nhật: bỏ qua chính tiện nghi đang sửa
    public async Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
        await _context.Amenities.AnyAsync(x =>
            x.Name == name && (excludeId == null || x.Id != excludeId));

    // Kiểm tra qua quan hệ n-n: có phòng nào đang dùng tiện nghi này không
    public async Task<bool> IsUsedByRoomsAsync(int id) =>
        await _context.Rooms.AnyAsync(r => r.Amenities.Any(a => a.Id == id));

    public async Task AddAsync(Amenity entity) =>
        await _context.Amenities.AddAsync(entity);

    public void Remove(Amenity entity) =>
        _context.Amenities.Remove(entity);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}