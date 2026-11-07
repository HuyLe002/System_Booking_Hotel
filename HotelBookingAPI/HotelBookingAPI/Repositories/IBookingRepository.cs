using HotelBookingAPI.Common;
using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface IBookingRepository
{
    Task<(List<Booking> Items, int TotalItems)> GetPagedAsync(BookingQuery query, string sortBy, bool desc);
    Task<Booking?> GetByIdAsync(int id);                   // chỉ đọc, kèm Customer, Room, RoomType, dịch vụ, mã giảm giá, giao dịch
    Task<Booking?> GetForUpdateAsync(int id);              // có tracking
    Task<int?> GetRoomIdAsync(int id);
    Task<Booking?> GetForUpdateWithRoomAsync(int id);      // có tracking, kèm Room, RoomType và dịch vụ
    Task<Booking?> GetForUpdateWithPaymentsAsync(int id);  // có tracking, kèm giao dịch (để hoàn tiền khi hủy)
    Task<bool> BookingHasPaymentHistoryAsync(int id);
    Task<bool> HasPendingPaymentClaimAsync(int id);
    Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId);
    Task<List<ExtraService>> GetExtraServicesByIdsAsync(List<int> ids);
    Task<Coupon?> GetCouponByCodeAsync(string code);       // có tracking (sẽ tăng UsedCount)
    Task<Coupon?> GetCouponForUpdateAsync(int id);         // có tracking (hoàn lượt khi hủy)
    Task<bool> CustomerUsedCouponAsync(int customerId, int couponId);
    Task<bool> CustomerExistsAsync(int customerId);
    Task<Room?> GetRoomWithTypeAsync(int roomId);
    Task<bool> HasOverlapAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null);
    Task AddAsync(Booking entity);
    void Remove(Booking entity);
    Task SaveChangesAsync();
}