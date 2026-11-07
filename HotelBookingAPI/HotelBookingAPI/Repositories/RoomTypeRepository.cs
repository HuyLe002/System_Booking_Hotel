using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class RoomTypeRepository : IRoomTypeRepository
{
    private readonly HotelDbContext _context;

    public RoomTypeRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoomType>> GetAllAsync() =>
        await _context.RoomTypes.AsNoTracking().OrderBy(x => x.Id).ToListAsync();

    public async Task<RoomType?> GetByIdAsync(int id) =>
        await _context.RoomTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<RoomType?> GetForUpdateAsync(int id) =>
        await _context.RoomTypes.FindAsync(id);

    // excludeId dùng khi cập nhật: bỏ qua chính bản ghi đang sửa
    public async Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
        await _context.RoomTypes.AnyAsync(x =>
            x.Name == name && (excludeId == null || x.Id != excludeId));

    public async Task<bool> HasRoomsAsync(int id) =>
        await _context.Rooms.AnyAsync(x => x.RoomTypeId == id);

    public async Task AddAsync(RoomType entity) =>
        await _context.RoomTypes.AddAsync(entity);

    public void Remove(RoomType entity) =>
        _context.RoomTypes.Remove(entity);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}