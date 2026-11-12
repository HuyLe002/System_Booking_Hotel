using HotelBookingAPI.Common;
using HotelBookingAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class StatisticsRepository : IStatisticsRepository
{
    private readonly HotelDbContext _context;

    public StatisticsRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<SummaryData> GetSummaryAsync()
    {
        var totalRooms = await _context.Rooms.CountAsync();
        var totalCustomers = await _context.Customers.CountAsync();
        var totalBookings = await _context.Bookings.CountAsync();

        // Gán ra biến cục bộ để EF dịch thành "Status IN (...)"
        var revenueStatuses = BookingStatuses.Revenue;

        // SUM của tập rỗng trả về null nên ép kiểu decimal? rồi ?? 0
        var totalRevenue = await _context.Bookings
            .Where(b => revenueStatuses.Contains(b.Status))
            .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

        return new SummaryData(totalRooms, totalCustomers, totalBookings, totalRevenue);
    }

    public async Task<List<MonthlyRevenueRow>> GetRevenueByMonthAsync(int year)
    {
        var revenueStatuses = BookingStatuses.Revenue;

        var data = await _context.Bookings
            .AsNoTracking()
            .Where(b => revenueStatuses.Contains(b.Status) && b.CheckInDate.Year == year)
            .GroupBy(b => b.CheckInDate.Month)
            .Select(g => new
            {
                Month = g.Key,
                TotalBookings = g.Count(),
                Revenue = g.Sum(b => b.TotalPrice)
            })
            .ToListAsync();

        return data
            .Select(d => new MonthlyRevenueRow(d.Month, d.TotalBookings, d.Revenue))
            .ToList();
    }

    public async Task<List<BookingStatusRow>> GetBookingsByStatusAsync()
    {
        var data = await _context.Bookings
            .AsNoTracking()
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return data
            .Select(d => new BookingStatusRow(d.Status, d.Count))
            .ToList();
    }

    public async Task<List<TopRoomRow>> GetTopRoomsAsync(int top)
    {
        var data = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Status != "Cancelled")
            .GroupBy(b => new
            {
                b.RoomId,
                b.Room!.RoomNumber,
                RoomTypeName = b.Room.RoomType!.Name
            })
            .Select(g => new
            {
                g.Key.RoomId,
                g.Key.RoomNumber,
                g.Key.RoomTypeName,
                TotalBookings = g.Count(),
                TotalNights = g.Sum(b => (int)(b.CheckOutDate - b.CheckInDate).TotalDays),
                Revenue = g.Sum(b => b.TotalPrice)
            })
            .OrderByDescending(x => x.TotalBookings)
            .ThenByDescending(x => x.Revenue)
            .Take(top)
            .ToListAsync();

        return data
            .Select(d => new TopRoomRow(d.RoomId, d.RoomNumber, d.RoomTypeName, d.TotalBookings, d.TotalNights, d.Revenue))
            .ToList();
    }

    public async Task<List<TopCustomerRow>> GetTopCustomersAsync(int top)
    {
        var data = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Status != "Cancelled")
            .GroupBy(b => new { b.CustomerId, b.Customer!.FullName })
            .Select(g => new
            {
                g.Key.CustomerId,
                g.Key.FullName,
                TotalBookings = g.Count(),
                TotalSpent = g.Sum(b => b.TotalPrice)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(top)
            .ToListAsync();

        return data
            .Select(d => new TopCustomerRow(d.CustomerId, d.FullName, d.TotalBookings, d.TotalSpent))
            .ToList();
    }
}