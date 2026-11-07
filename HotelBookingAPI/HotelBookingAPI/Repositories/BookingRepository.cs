using HotelBookingAPI.Common;
using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly HotelDbContext _context;

    public BookingRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Booking> Items, int TotalItems)> GetPagedAsync(
        BookingQuery query, string sortBy, bool desc)
    {
        // 1. Lọc
        var bookings = _context.Bookings.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
            bookings = bookings.Where(x => x.Status == query.Status);

        if (query.CustomerId.HasValue)
            bookings = bookings.Where(x => x.CustomerId == query.CustomerId.Value);

        if (query.RoomId.HasValue)
            bookings = bookings.Where(x => x.RoomId == query.RoomId.Value);

        if (query.From.HasValue)
            bookings = bookings.Where(x => x.CheckInDate >= query.From.Value.Date);

        if (query.To.HasValue)
            bookings = bookings.Where(x => x.CheckInDate <= query.To.Value.Date);

        // 2. Đếm tổng (trước khi phân trang)
        var totalItems = await bookings.CountAsync();

        // 3. Sắp xếp (thêm khóa phụ Id để thứ tự luôn xác định)
        bookings = (sortBy, desc) switch
        {
            ("checkout", false) => bookings.OrderBy(x => x.CheckOutDate).ThenBy(x => x.Id),
            ("checkout", true) => bookings.OrderByDescending(x => x.CheckOutDate).ThenBy(x => x.Id),
            ("price", false) => bookings.OrderBy(x => x.TotalPrice).ThenBy(x => x.Id),
            ("price", true) => bookings.OrderByDescending(x => x.TotalPrice).ThenBy(x => x.Id),
            ("status", false) => bookings.OrderBy(x => x.Status).ThenBy(x => x.Id),
            ("status", true) => bookings.OrderByDescending(x => x.Status).ThenBy(x => x.Id),
            (_, true) => bookings.OrderByDescending(x => x.CheckInDate).ThenBy(x => x.Id),
            _ => bookings.OrderBy(x => x.CheckInDate).ThenBy(x => x.Id)
        };

        // 4. Phân trang rồi mới Include. Có hai collection (dịch vụ, giao dịch) nên tách truy vấn
        var items = await bookings
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(x => x.Customer)
            .Include(x => x.Coupon)
            .Include(x => x.Room)
                .ThenInclude(r => r!.RoomType)
            .Include(x => x.ExtraServices)
                .ThenInclude(s => s.ExtraService)
            .Include(x => x.Payments)
            .Include(x => x.PaymentClaims)
            .AsSplitQuery()
            .ToListAsync();

        return (items, totalItems);
    }

    public async Task<Booking?> GetByIdAsync(int id) =>
        await _context.Bookings
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Coupon)
            .Include(x => x.Room)
                .ThenInclude(r => r!.RoomType)
            .Include(x => x.ExtraServices)
                .ThenInclude(s => s.ExtraService)
            .Include(x => x.Payments)
            .Include(x => x.PaymentClaims)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Booking?> GetForUpdateAsync(int id) =>
        await _context.Bookings.FindAsync(id);

    public async Task<int?> GetRoomIdAsync(int id) =>
        await _context.Bookings
            .Where(x => x.Id == id)
            .Select(x => (int?)x.RoomId)
            .FirstOrDefaultAsync();

    public async Task<Booking?> GetForUpdateWithRoomAsync(int id) =>
        await _context.Bookings
            .Include(x => x.Room)
                .ThenInclude(r => r!.RoomType)
            .Include(x => x.ExtraServices)
            .Include(x => x.PaymentClaims)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Booking?> GetForUpdateWithPaymentsAsync(int id) =>
        await _context.Bookings
            .Include(x => x.Payments)
            .Include(x => x.PaymentClaims)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<bool> BookingHasPaymentHistoryAsync(int id) =>
        await _context.Payments.AnyAsync(x => x.BookingId == id)
        || await _context.PaymentClaims.AnyAsync(x => x.BookingId == id);

    public async Task<bool> HasPendingPaymentClaimAsync(int id) =>
        await _context.PaymentClaims.AnyAsync(x =>
            x.BookingId == id && x.Status == PaymentClaimStatuses.Pending);

    public async Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId)
    {
        var query = _context.Rooms
            .AsNoTracking()
            .Where(r => r.Status == "Available")
            // Loại các phòng đã có booking trùng khoảng ngày (bỏ qua booking đã hủy)
            .Where(r => !r.Bookings.Any(b =>
                b.Status != "Cancelled" &&
                b.CheckInDate < checkOut &&
                b.CheckOutDate > checkIn));

        if (roomTypeId.HasValue)
            query = query.Where(r => r.RoomTypeId == roomTypeId.Value);

        return await query
            .OrderBy(r => r.RoomNumber)
            .Include(r => r.RoomType)
            .Include(r => r.Amenities)
            .ToListAsync();
    }

    // Chỉ đọc: ta chỉ cần giá và trạng thái, còn dòng booking lưu theo ExtraServiceId
    public async Task<List<ExtraService>> GetExtraServicesByIdsAsync(List<int> ids) =>
        await _context.ExtraServices
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

    // Có tracking: Service sẽ tăng UsedCount, SaveChanges kèm RowVersion để chống tranh chấp
    public async Task<Coupon?> GetCouponByCodeAsync(string code) =>
        await _context.Coupons.FirstOrDefaultAsync(x => x.Code == code);

    public async Task<Coupon?> GetCouponForUpdateAsync(int id) =>
        await _context.Coupons.FirstOrDefaultAsync(x => x.Id == id);

    // Booking đã hủy không tính, nên khách hủy rồi có thể dùng lại mã
    public async Task<bool> CustomerUsedCouponAsync(int customerId, int couponId) =>
        await _context.Bookings.AnyAsync(b =>
            b.CustomerId == customerId &&
            b.CouponId == couponId &&
            b.Status != "Cancelled");

    public async Task<bool> CustomerExistsAsync(int customerId) =>
        await _context.Customers.AnyAsync(x => x.Id == customerId);

    public async Task<Room?> GetRoomWithTypeAsync(int roomId) =>
        await _context.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .FirstOrDefaultAsync(x => x.Id == roomId);

    // Công thức trùng lịch: existing.CheckIn < new.CheckOut && existing.CheckOut > new.CheckIn
    public async Task<bool> HasOverlapAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null) =>
        await _context.Bookings.AnyAsync(b =>
            b.RoomId == roomId &&
            b.Status != "Cancelled" &&
            (excludeBookingId == null || b.Id != excludeBookingId) &&
            b.CheckInDate < checkOut &&
            b.CheckOutDate > checkIn);

    public async Task AddAsync(Booking entity) =>
        await _context.Bookings.AddAsync(entity);

    public void Remove(Booking entity) =>
        _context.Bookings.Remove(entity);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}