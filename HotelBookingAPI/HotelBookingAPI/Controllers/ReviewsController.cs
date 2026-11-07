using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewsController(IReviewService service)
    {
        _service = service;
    }

    // Đọc thông tin người gọi từ token.
    // Tham số đầu = "có quyền kiểm duyệt": Admin và Staff được xóa đánh giá của người khác
    private CurrentUser CurrentUserInfo => new(
        User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Staff),
        int.TryParse(User.FindFirst("customerId")?.Value, out var id) ? id : null);

    // GET: api/Reviews/room/1?page=1&pageSize=5&sortBy=rating&sortDir=desc
    [AllowAnonymous]
    [HttpGet("room/{roomId:int}")]
    public async Task<ActionResult<RoomReviewsDto>> GetByRoom(int roomId, [FromQuery] PagingQuery query)
    {
        return Ok(await _service.GetByRoomAsync(roomId, query));
    }

    // GET: api/Reviews/5
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReviewDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    // POST: api/Reviews  (khách đăng nhập, chỉ cho booking của mình)
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(CreateReviewDto dto)
    {
        var created = await _service.CreateAsync(dto, CurrentUserInfo);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Reviews/5  (chỉ chủ đánh giá)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateReviewDto dto)
    {
        await _service.UpdateAsync(id, dto, CurrentUserInfo);
        return NoContent();
    }

    // DELETE: api/Reviews/5  (chủ đánh giá, Staff hoặc Admin)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id, CurrentUserInfo);
        return NoContent();
    }
}