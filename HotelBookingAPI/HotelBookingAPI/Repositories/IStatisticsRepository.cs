namespace HotelBookingAPI.Repositories;

public interface IStatisticsRepository
{
    Task<SummaryData> GetSummaryAsync();
    Task<List<MonthlyRevenueRow>> GetRevenueByMonthAsync(int year);
    Task<List<BookingStatusRow>> GetBookingsByStatusAsync();
    Task<List<TopRoomRow>> GetTopRoomsAsync(int top);
    Task<List<TopCustomerRow>> GetTopCustomersAsync(int top);
}