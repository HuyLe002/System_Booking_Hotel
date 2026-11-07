using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize(Policy = Policies.StaffOrAdmin)]
[Route("api/PaymentClaims")]
[ApiController]
public class PaymentClaimsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentClaimsController(IPaymentService service)
    {
        _service = service;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<PagedResult<PaymentClaimQueueItemDto>>> GetPending(
        [FromQuery] PagingQuery query)
    {
        return Ok(await _service.GetPendingClaimsAsync(query.Page, query.PageSize));
    }
}
