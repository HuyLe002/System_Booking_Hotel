using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface IStatisticsService
{
    Task<SummaryDto> GetSummaryAsync();
    Task<List<MonthlyRevenueDto>> GetRevenueByMonthAsync(int year);
    Task<List<BookingStatusStatDto>> GetBookingsByStatusAsync();
    Task<List<TopRoomDto>> GetTopRoomsAsync(int top);
    Task<List<TopCustomerDto>> GetTopCustomersAsync(int top);
}