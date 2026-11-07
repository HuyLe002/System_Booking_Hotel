using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _service;

    public RoomsController(IRoomService service)
    {
        _service = service;
    }

    // GET: api/Rooms?status=Available&roomTypeId=1&keyword=10&page=1&pageSize=5&sortBy=price&sortDir=desc
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<RoomDto>>> GetAll([FromQuery] RoomQuery query)
    {
        return Ok(await _service.GetPagedAsync(query));
    }

    // GET: api/Rooms/5
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<RoomDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    // POST: api/Rooms
    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create(CreateRoomDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Rooms/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoomDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    // DELETE: api/Rooms/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}