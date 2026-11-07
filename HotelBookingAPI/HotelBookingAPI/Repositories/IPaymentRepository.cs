using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface IPaymentRepository
{
    Task<Booking?> GetBookingWithPaymentsAsync(int bookingId);   // chỉ đọc, kèm toàn bộ giao dịch
    Task<Booking?> GetBookingWithPaymentsForUpdateAsync(int bookingId);
    Task<(List<PaymentClaim> Items, int TotalItems)> GetPendingClaimsAsync(int page, int pageSize);
    Task AddAsync(Payment payment);
    Task SaveChangesAsync();
}