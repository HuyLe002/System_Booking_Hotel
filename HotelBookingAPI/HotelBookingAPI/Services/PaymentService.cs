using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class PaymentService : IPaymentService
{
    private const string NoPermission = "Bạn không có quyền thực hiện thao tác này";

    private readonly IPaymentRepository _repo;
    private readonly IBookingLock _bookingLock;
    private readonly IAuditService _audit;

    public PaymentService(IPaymentRepository repo, IBookingLock bookingLock, IAuditService audit)
    {
        _repo = repo;
        _bookingLock = bookingLock;
        _audit = audit;
    }

    public async Task<BookingPaymentsDto> GetByBookingAsync(int bookingId, CurrentUser user)
    {
        var booking = await GetBookingAsync(bookingId);

        if (!user.IsAdmin && user.CustomerId != booking.CustomerId)
            throw new UnauthorizedAccessException(NoPermission);

        var paid = PaymentRules.TotalPaid(booking.Payments);
        var refunded = PaymentRules.TotalRefunded(booking.Payments);

        return new BookingPaymentsDto
        {
            BookingId = booking.Id,
            BookingStatus = booking.Status,
            RoomNumber = booking.Room?.RoomNumber ?? string.Empty,
            RoomTypeName = booking.Room?.RoomType?.Name ?? string.Empty,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            TotalPrice = booking.TotalPrice,
            TotalPaid = paid,
            TotalRefunded = refunded,
            BalanceDue = PaymentRules.BalanceDue(booking.TotalPrice, paid - refunded, booking.Status),
            PaymentStatus = PaymentRules.GetStatus(booking.TotalPrice, paid, refunded, booking.Status),
            Items = booking.Payments
                .OrderBy(p => p.CreatedAt).ThenBy(p => p.Id)
                .Select(ToDto)
                .ToList(),
            PaymentClaims = booking.PaymentClaims
                .OrderByDescending(c => c.CustomerConfirmedAt)
                .Select(ToDto)
                .ToList()
        };
    }

    public async Task<PagedResult<PaymentClaimQueueItemDto>> GetPendingClaimsAsync(int page, int pageSize)
    {
        var (items, totalItems) = await _repo.GetPendingClaimsAsync(page, pageSize);

        return new PagedResult<PaymentClaimQueueItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items.Select(x => new PaymentClaimQueueItemDto
            {
                Id = x.Id,
                BookingId = x.BookingId,
                Amount = x.Amount,
                Method = x.Method,
                Note = x.Note,
                Status = x.Status,
                CustomerConfirmedAt = x.CustomerConfirmedAt,
                CashierReviewedAt = x.CashierReviewedAt,
                CashierUserId = x.CashierUserId,
                CashierNote = x.CashierNote,
                CustomerName = x.Booking?.Customer?.FullName ?? string.Empty,
                RoomNumber = x.Booking?.Room?.RoomNumber ?? string.Empty,
                RoomTypeName = x.Booking?.Room?.RoomType?.Name ?? string.Empty,
                CheckInDate = x.Booking?.CheckInDate ?? default,
                CheckOutDate = x.Booking?.CheckOutDate ?? default,
                BookingTotal = x.Booking?.TotalPrice ?? 0,
                BalanceDue = x.Booking == null
                    ? 0
                    : PaymentRules.BalanceDue(
                        x.Booking.TotalPrice,
                        PaymentRules.NetPaid(x.Booking.Payments),
                        x.Booking.Status)
            }).ToList()
        };
    }

    public async Task<PaymentClaimDto> SubmitClaimAsync(
        int bookingId, CreatePaymentClaimDto dto, CurrentUser user)
    {
        await using var tx = await _bookingLock.BeginBookingLockAsync(bookingId);
        var booking = await GetBookingForUpdateAsync(bookingId);

        if (user.IsAdmin || user.CustomerId != booking.CustomerId)
            throw new UnauthorizedAccessException(NoPermission);

        if (booking.Status == "Cancelled")
            throw new ArgumentException("Booking đã hủy, không thể gửi xác nhận thanh toán");

        if (booking.PaymentClaims.Any(x => x.Status == PaymentClaimStatuses.Pending))
            throw new ArgumentException("Booking đang có yêu cầu thanh toán chờ thu ngân xác nhận");

        var balance = PaymentRules.BalanceDue(
            booking.TotalPrice,
            PaymentRules.NetPaid(booking.Payments),
            booking.Status);

        if (balance <= 0)
            throw new ArgumentException("Booking đã thanh toán đủ");

        if (dto.Amount > balance)
            throw new ArgumentException($"Số tiền vượt quá số còn phải trả ({balance:N0} đ)");

        var claim = new PaymentClaim
        {
            BookingId = bookingId,
            Amount = dto.Amount,
            Method = dto.Method,
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
            Status = PaymentClaimStatuses.Pending,
            CustomerConfirmedAt = DateTime.UtcNow
        };

        booking.PaymentClaims.Add(claim);
        await _repo.SaveChangesAsync();

        _audit.Log(AuditActions.PaymentClaimSubmitted, "PaymentClaim", claim.Id,
            $"Booking {bookingId}, khách xác nhận đã trả {claim.Amount:N0} đ bằng {claim.Method}");

        await _repo.SaveChangesAsync();
        await tx.CommitAsync();
        return ToDto(claim);
    }

    public async Task<PaymentDto> ConfirmClaimAsync(
        int bookingId, int claimId, int cashierUserId, ReviewPaymentClaimDto? dto)
    {
        await using var tx = await _bookingLock.BeginBookingLockAsync(bookingId);
        var booking = await GetBookingForUpdateAsync(bookingId);
        var claim = GetPendingClaim(booking, claimId);

        if (booking.Status == "Cancelled")
            throw new ArgumentException("Booking đã hủy, không thể xác nhận thanh toán");

        var balance = PaymentRules.BalanceDue(
            booking.TotalPrice,
            PaymentRules.NetPaid(booking.Payments),
            booking.Status);

        if (claim.Amount > balance)
            throw new ArgumentException($"Số tiền vượt quá số còn phải trả ({balance:N0} đ)");

        var now = DateTime.UtcNow;
        claim.Status = PaymentClaimStatuses.Confirmed;
        claim.CashierReviewedAt = now;
        claim.CashierUserId = cashierUserId;
        claim.CashierNote = NormalizeNote(dto?.Note);

        var payment = new Payment
        {
            BookingId = bookingId,
            Amount = claim.Amount,
            Type = PaymentRules.TypePayment,
            Method = claim.Method,
            Note = claim.Note,
            CreatedAt = now
        };
        booking.Payments.Add(payment);

        await _repo.SaveChangesAsync();
        _audit.Log(AuditActions.PaymentClaimConfirmed, "PaymentClaim", claim.Id,
            $"Thu ngân xác nhận booking {bookingId} đã nhận {claim.Amount:N0} đ");

        await _repo.SaveChangesAsync();
        await tx.CommitAsync();
        return ToDto(payment);
    }

    public async Task<PaymentClaimDto> RejectClaimAsync(
        int bookingId, int claimId, int cashierUserId, ReviewPaymentClaimDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Note))
            throw new ArgumentException("Vui lòng nhập lý do từ chối xác nhận thanh toán");

        await using var tx = await _bookingLock.BeginBookingLockAsync(bookingId);
        var booking = await GetBookingForUpdateAsync(bookingId);
        var claim = GetPendingClaim(booking, claimId);

        claim.Status = PaymentClaimStatuses.Rejected;
        claim.CashierReviewedAt = DateTime.UtcNow;
        claim.CashierUserId = cashierUserId;
        claim.CashierNote = dto.Note.Trim();

        _audit.Log(AuditActions.PaymentClaimRejected, "PaymentClaim", claim.Id,
            $"Thu ngân từ chối booking {bookingId}: {claim.CashierNote}");

        await _repo.SaveChangesAsync();
        await tx.CommitAsync();
        return ToDto(claim);
    }

    public async Task<PaymentDto> RecordRefundAsync(int bookingId, CreatePaymentDto dto)
    {
        await using var tx = await _bookingLock.BeginBookingLockAsync(bookingId);
        var booking = await GetBookingForUpdateAsync(bookingId);
        var netPaid = PaymentRules.NetPaid(booking.Payments);

        if (dto.Amount > netPaid)
            throw new ArgumentException($"Số tiền hoàn vượt quá số đã thu ({netPaid:N0} đ)");

        var payment = new Payment
        {
            BookingId = bookingId,
            Amount = dto.Amount,
            Type = PaymentRules.TypeRefund,
            Method = dto.Method,
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        booking.Payments.Add(payment);
        await _repo.SaveChangesAsync();
        _audit.Log(AuditActions.RefundRecorded, "Payment", payment.Id,
            $"Booking {bookingId}, hoàn {payment.Amount:N0} đ");

        await _repo.SaveChangesAsync();
        await tx.CommitAsync();
        return ToDto(payment);
    }

    private async Task<Booking> GetBookingAsync(int bookingId) =>
        await _repo.GetBookingWithPaymentsAsync(bookingId)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {bookingId}");

    private async Task<Booking> GetBookingForUpdateAsync(int bookingId) =>
        await _repo.GetBookingWithPaymentsForUpdateAsync(bookingId)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {bookingId}");

    private static PaymentClaim GetPendingClaim(Booking booking, int claimId)
    {
        var claim = booking.PaymentClaims.FirstOrDefault(x => x.Id == claimId)
            ?? throw new KeyNotFoundException($"Không tìm thấy yêu cầu thanh toán có Id = {claimId}");

        if (claim.Status != PaymentClaimStatuses.Pending)
            throw new ArgumentException("Yêu cầu thanh toán không còn ở trạng thái chờ xác nhận");

        return claim;
    }

    private static string? NormalizeNote(string? note) =>
        string.IsNullOrWhiteSpace(note) ? null : note.Trim();

    private static PaymentDto ToDto(Payment x) => new()
    {
        Id = x.Id,
        BookingId = x.BookingId,
        Amount = x.Amount,
        Type = x.Type,
        Method = x.Method,
        Note = x.Note,
        CreatedAt = x.CreatedAt
    };

    private static PaymentClaimDto ToDto(PaymentClaim x) => new()
    {
        Id = x.Id,
        BookingId = x.BookingId,
        Amount = x.Amount,
        Method = x.Method,
        Note = x.Note,
        Status = x.Status,
        CustomerConfirmedAt = x.CustomerConfirmedAt,
        CashierReviewedAt = x.CashierReviewedAt,
        CashierUserId = x.CashierUserId,
        CashierNote = x.CashierNote
    };
}
