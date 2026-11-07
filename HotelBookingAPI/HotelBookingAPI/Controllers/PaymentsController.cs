using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBookingAPI.Controllers;

// Địa chỉ dạng api/Bookings/{bookingId}/payments: thanh toán là tài nguyên con của booking
[Authorize]
[Route("api/Bookings/{bookingId:int}")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service)
    {
        _service = service;
    }

    // Admin và Staff đều có quyền xem mọi booking
    private CurrentUser CurrentUserInfo => new(
        User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Staff),
        int.TryParse(User.FindFirst("customerId")?.Value, out var id) ? id : null);

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // GET: api/Bookings/5/payments  (chủ booking, Staff hoặc Admin)
    [HttpGet("payments")]
    public async Task<ActionResult<BookingPaymentsDto>> GetByBooking(int bookingId)
    {
        return Ok(await _service.GetByBookingAsync(bookingId, CurrentUserInfo));
    }

    // POST: api/Bookings/5/payment-claims  (khách xác nhận đã thanh toán)
    [HttpPost("payment-claims")]
    public async Task<ActionResult<PaymentClaimDto>> SubmitClaim(
        int bookingId, CreatePaymentClaimDto dto)
    {
        var claim = await _service.SubmitClaimAsync(bookingId, dto, CurrentUserInfo);
        return CreatedAtAction(nameof(GetByBooking), new { bookingId }, claim);
    }

    // POST: api/Bookings/5/payment-claims/7/confirm  (thu ngân đối soát tiền đã nhận)
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [HttpPost("payment-claims/{claimId:int}/confirm")]
    public async Task<ActionResult<PaymentDto>> ConfirmClaim(
        int bookingId, int claimId, ReviewPaymentClaimDto? dto)
    {
        var payment = await _service.ConfirmClaimAsync(bookingId, claimId, CurrentUserId, dto);
        return Ok(payment);
    }

    // POST: api/Bookings/5/payment-claims/7/reject  (thu ngân từ chối, bắt buộc có lý do)
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [HttpPost("payment-claims/{claimId:int}/reject")]
    public async Task<ActionResult<PaymentClaimDto>> RejectClaim(
        int bookingId, int claimId, ReviewPaymentClaimDto dto)
    {
        return Ok(await _service.RejectClaimAsync(bookingId, claimId, CurrentUserId, dto));
    }

    // Giữ route cũ để client biết phải chuyển sang luồng xác nhận hai bước.
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [HttpPost("payments")]
    public IActionResult RecordPayment(int bookingId)
    {
        return StatusCode(StatusCodes.Status410Gone, new
        {
            message = "Ghi nhận trực tiếp đã ngừng hỗ trợ. Khách cần gửi payment-claims, sau đó thu ngân xác nhận."
        });
    }

    // POST: api/Bookings/5/refunds  (chỉ Admin, hoàn tiền thủ công)
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("refunds")]
    public async Task<ActionResult<PaymentDto>> RecordRefund(int bookingId, CreatePaymentDto dto)
    {
        var created = await _service.RecordRefundAsync(bookingId, dto);
        return CreatedAtAction(nameof(GetByBooking), new { bookingId }, created);
    }
}