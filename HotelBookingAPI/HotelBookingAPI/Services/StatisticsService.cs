using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IStatisticsRepository _repo;

    public StatisticsService(IStatisticsRepository repo)
    {
        _repo = repo;
    }

    public async Task<SummaryDto> GetSummaryAsync()
    {
        var data = await _repo.GetSummaryAsync();

        return new SummaryDto
        {
            TotalRooms = data.TotalRooms,
            TotalCustomers = data.TotalCustomers,
            TotalBookings = data.TotalBookings,
            TotalRevenue = data.TotalRevenue
        };
    }

    public async Task<List<MonthlyRevenueDto>> GetRevenueByMonthAsync(int year)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentException("Năm không hợp lệ");

        var rows = await _repo.GetRevenueByMonthAsync(year);

        // Đủ 12 tháng, tháng không có booking thì doanh thu = 0
        return Enumerable.Range(1, 12)
            .Select(m =>
            {
                var row = rows.FirstOrDefault(r => r.Month == m);
                return new MonthlyRevenueDto
                {
                    Month = m,
                    TotalBookings = row?.TotalBookings ?? 0,
                    Revenue = row?.Revenue ?? 0
                };
            })
            .ToList();
    }

    public async Task<List<BookingStatusStatDto>> GetBookingsByStatusAsync()
    {
        var rows = await _repo.GetBookingsByStatusAsync();

        // Đủ mọi trạng thái, trạng thái chưa có booking thì Count = 0
        return BookingStatuses.All
            .Select(s => new BookingStatusStatDto
            {
                Status = s,
                Count = rows.FirstOrDefault(r => r.Status == s)?.Count ?? 0
            })
            .ToList();
    }

    public async Task<List<TopRoomDto>> GetTopRoomsAsync(int top)
    {
        ValidateTop(top);

        var rows = await _repo.GetTopRoomsAsync(top);
        return rows.Select(r => new TopRoomDto
        {
            RoomId = r.RoomId,
            RoomNumber = r.RoomNumber,
            RoomTypeName = r.RoomTypeName,
            TotalBookings = r.TotalBookings,
            TotalNights = r.TotalNights,
            Revenue = r.Revenue
        }).ToList();
    }

    public async Task<List<TopCustomerDto>> GetTopCustomersAsync(int top)
    {
        ValidateTop(top);

        var rows = await _repo.GetTopCustomersAsync(top);
        return rows.Select(r => new TopCustomerDto
        {
            CustomerId = r.CustomerId,
            FullName = r.FullName,
            TotalBookings = r.TotalBookings,
            TotalSpent = r.TotalSpent
        }).ToList();
    }

    private static void ValidateTop(int top)
    {
        if (top < 1 || top > 50)
            throw new ArgumentException("top phải từ 1 đến 50");
    }
}