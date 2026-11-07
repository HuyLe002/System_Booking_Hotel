using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class RoomTypesController : ControllerBase
{
    private readonly IRoomTypeService _service;

    public RoomTypesController(IRoomTypeService service)
    {
        _service = service;
    }

    // GET: api/RoomTypes
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RoomTypeDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    // GET: api/RoomTypes/5
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<RoomTypeDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    // POST: api/RoomTypes
    [HttpPost]
    public async Task<ActionResult<RoomTypeDto>> Create(CreateRoomTypeDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/RoomTypes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoomTypeDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    // DELETE: api/RoomTypes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}