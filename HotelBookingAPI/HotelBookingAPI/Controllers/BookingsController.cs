using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service)
    {
        _service = service;
    }

    // Đọc thông tin người gọi từ token.
    // Tham số đầu = "có quyền thao tác mọi booking": đúng với Admin và Staff
    private CurrentUser CurrentUserInfo => new(
        User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Staff),
        int.TryParse(User.FindFirst("customerId")?.Value, out var id) ? id : null);

    // GET: api/Bookings?status=Pending&roomId=1&from=2026-11-01&to=2026-11-30&page=1&pageSize=5&sortBy=price&sortDir=desc
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookingDto>>> GetAll([FromQuery] BookingQuery query)
    {
        return Ok(await _service.GetPagedAsync(query, CurrentUserInfo));
    }

    // GET: api/Bookings/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id, CurrentUserInfo));
    }

    // GET: api/Bookings/available-rooms?checkIn=2026-11-01&checkOut=2026-11-04&roomTypeId=1
    [AllowAnonymous]
    [HttpGet("available-rooms")]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAvailableRooms(
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        [FromQuery] int? roomTypeId)
    {
        return Ok(await _service.GetAvailableRoomsAsync(checkIn, checkOut, roomTypeId));
    }

    // POST: api/Bookings  (Staff và Admin đặt hộ khách bằng customerId)
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingDto dto)
    {
        var created = await _service.CreateAsync(dto, CurrentUserInfo);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Bookings/5/dates  (chủ booking, Staff hoặc Admin, chỉ khi còn Pending)
    [HttpPut("{id:int}/dates")]
    public async Task<IActionResult> UpdateDates(int id, UpdateBookingDatesDto dto)
    {
        await _service.UpdateDatesAsync(id, dto, CurrentUserInfo);
        return NoContent();
    }

    // PUT: api/Bookings/5/services  (chủ booking, Staff hoặc Admin)
    [HttpPut("{id:int}/services")]
    public async Task<IActionResult> UpdateServices(int id, UpdateBookingServicesDto dto)
    {
        await _service.UpdateServicesAsync(id, dto, CurrentUserInfo);
        return NoContent();
    }

    // PUT: api/Bookings/5/confirm  (Staff hoặc Admin)
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        await _service.ConfirmAsync(id);
        return NoContent();
    }

    // PUT: api/Bookings/5/check-in  (Staff hoặc Admin, cần đã cọc đủ và đúng ngày)
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [HttpPut("{id:int}/check-in")]
    public async Task<IActionResult> CheckIn(int id)
    {
        await _service.CheckInAsync(id);
        return NoContent();
    }

    // PUT: api/Bookings/5/check-out  (Staff hoặc Admin, cần đã thanh toán đủ)
    [Authorize(Policy = Policies.StaffOrAdmin)]
    [HttpPut("{id:int}/check-out")]
    public async Task<IActionResult> CheckOut(int id)
    {
        await _service.CheckOutAsync(id);
        return NoContent();
    }

    // PUT: api/Bookings/5/cancel  (chủ booking, Staff hoặc Admin)
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelAsync(id, CurrentUserInfo);
        return NoContent();
    }

    // DELETE: api/Bookings/5  (chỉ Admin, chỉ xóa booking đã hủy)
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}