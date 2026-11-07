using HotelBookingAPI.Common;
using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly HotelDbContext _context;

    public ReviewRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Review> Items, int TotalItems)> GetPagedByRoomAsync(
        int roomId, PagingQuery query, string sortBy, bool desc)
    {
        // 1. Lọc
        var reviews = _context.Reviews.AsNoTracking().Where(x => x.RoomId == roomId);

        // 2. Đếm tổng (trước khi phân trang)
        var totalItems = await reviews.CountAsync();

        // 3. Sắp xếp (thêm khóa phụ Id)
        reviews = (sortBy, desc) switch
        {
            ("rating", false) => reviews.OrderBy(x => x.Rating).ThenBy(x => x.Id),
            ("rating", true) => reviews.OrderByDescending(x => x.Rating).ThenBy(x => x.Id),
            (_, false) => reviews.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            _ => reviews.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
        };

        // 4. Phân trang rồi mới Include
        var items = await reviews
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(x => x.Room)
            .Include(x => x.Customer)
            .ToListAsync();

        return (items, totalItems);
    }

    public async Task<RoomRatingSummary> GetRatingSummaryAsync(int roomId)
    {
        var reviews = _context.Reviews.AsNoTracking().Where(x => x.RoomId == roomId);

        var count = await reviews.CountAsync();

        // Ép kiểu sang double? để phòng chưa có đánh giá nào trả null thay vì ném lỗi
        var average = await reviews.AverageAsync(x => (double?)x.Rating) ?? 0;

        return new RoomRatingSummary(average, count);
    }

    public async Task<Review?> GetByIdAsync(int id) =>
        await _context.Reviews
            .AsNoTracking()
            .Include(x => x.Room)
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Review?> GetForUpdateAsync(int id) =>
        await _context.Reviews.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Room?> GetRoomAsync(int roomId) =>
        await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == roomId);

    public async Task<Booking?> GetBookingAsync(int bookingId) =>
        await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == bookingId);

    public async Task<bool> BookingHasReviewAsync(int bookingId) =>
        await _context.Reviews.AnyAsync(x => x.BookingId == bookingId);

    public async Task AddAsync(Review entity) =>
        await _context.Reviews.AddAsync(entity);

    public void Remove(Review entity) =>
        _context.Reviews.Remove(entity);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}