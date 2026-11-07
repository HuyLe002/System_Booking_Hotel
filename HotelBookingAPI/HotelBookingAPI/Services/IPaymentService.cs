using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface IPaymentService
{
    Task<BookingPaymentsDto> GetByBookingAsync(int bookingId, CurrentUser user);
    Task<PagedResult<PaymentClaimQueueItemDto>> GetPendingClaimsAsync(int page, int pageSize);
    Task<PaymentClaimDto> SubmitClaimAsync(int bookingId, CreatePaymentClaimDto dto, CurrentUser user);
    Task<PaymentDto> ConfirmClaimAsync(int bookingId, int claimId, int cashierUserId, ReviewPaymentClaimDto? dto);
    Task<PaymentClaimDto> RejectClaimAsync(int bookingId, int claimId, int cashierUserId, ReviewPaymentClaimDto dto);
    Task<PaymentDto> RecordRefundAsync(int bookingId, CreatePaymentDto dto);
}