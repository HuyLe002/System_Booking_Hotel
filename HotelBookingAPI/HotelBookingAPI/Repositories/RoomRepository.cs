using HotelBookingAPI.Common;
using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly HotelDbContext _context;

    public RoomRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Room> Items, int TotalItems)> GetPagedAsync(
        RoomQuery query, string sortBy, bool desc)
    {
        // 1. Lọc
        var rooms = _context.Rooms.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
            rooms = rooms.Where(x => x.Status == query.Status);

        if (query.RoomTypeId.HasValue)
            rooms = rooms.Where(x => x.RoomTypeId == query.RoomTypeId.Value);

        if (query.Floor.HasValue)
            rooms = rooms.Where(x => x.Floor == query.Floor.Value);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            rooms = rooms.Where(x => x.RoomNumber.Contains(query.Keyword));

        // 2. Đếm tổng (trước khi phân trang)
        var totalItems = await rooms.CountAsync();

        // 3. Sắp xếp (thêm khóa phụ Id để thứ tự luôn xác định)
        rooms = (sortBy, desc) switch
        {
            ("price", false) => rooms.OrderBy(x => x.RoomType!.PricePerNight).ThenBy(x => x.Id),
            ("price", true) => rooms.OrderByDescending(x => x.RoomType!.PricePerNight).ThenBy(x => x.Id),
            ("status", false) => rooms.OrderBy(x => x.Status).ThenBy(x => x.Id),
            ("status", true) => rooms.OrderByDescending(x => x.Status).ThenBy(x => x.Id),
            ("floor", false) => rooms.OrderBy(x => x.Floor).ThenBy(x => x.RoomNumber),
            ("floor", true) => rooms.OrderByDescending(x => x.Floor).ThenBy(x => x.RoomNumber),
            (_, true) => rooms.OrderByDescending(x => x.RoomNumber),
            _ => rooms.OrderBy(x => x.RoomNumber)
        };

        // 4. Phân trang rồi mới Include
        var items = await rooms
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(x => x.RoomType)
            .Include(x => x.Amenities)
            .ToListAsync();

        return (items, totalItems);
    }

    public async Task<Room?> GetByIdAsync(int id) =>
        await _context.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .Include(x => x.Amenities)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Room?> GetForUpdateAsync(int id) =>
        await _context.Rooms
            .Include(x => x.Amenities)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<bool> RoomTypeExistsAsync(int roomTypeId) =>
        await _context.RoomTypes.AnyAsync(x => x.Id == roomTypeId);

    // excludeId dùng khi cập nhật: bỏ qua chính phòng đang sửa
    public async Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeId = null) =>
        await _context.Rooms.AnyAsync(x =>
            x.RoomNumber == roomNumber && (excludeId == null || x.Id != excludeId));

    public async Task<List<Amenity>> GetAmenitiesByIdsAsync(List<int> ids) =>
        await _context.Amenities.Where(a => ids.Contains(a.Id)).ToListAsync();

    public async Task<bool> HasBookingsAsync(int roomId) =>
        await _context.Bookings.AnyAsync(x => x.RoomId == roomId);

    public async Task AddAsync(Room entity) =>
        await _context.Rooms.AddAsync(entity);

    // Xóa mềm: chỉ đánh dấu, không xóa khỏi database
    public void Remove(Room entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}