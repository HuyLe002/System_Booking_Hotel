using HotelBookingAPI.Common;
using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

// Dòng tổng hợp: điểm trung bình và số lượng đánh giá
public record RoomRatingSummary(double Average, int Count);

public interface IReviewRepository
{
    Task<(List<Review> Items, int TotalItems)> GetPagedByRoomAsync(int roomId, PagingQuery query, string sortBy, bool desc);
    Task<RoomRatingSummary> GetRatingSummaryAsync(int roomId);
    Task<Review?> GetByIdAsync(int id);              // chỉ đọc, kèm Room, Customer
    Task<Review?> GetForUpdateAsync(int id);         // có tracking
    Task<Room?> GetRoomAsync(int roomId);
    Task<Booking?> GetBookingAsync(int bookingId);
    Task<bool> BookingHasReviewAsync(int bookingId);
    Task AddAsync(Review entity);
    void Remove(Review entity);
    Task SaveChangesAsync();
}