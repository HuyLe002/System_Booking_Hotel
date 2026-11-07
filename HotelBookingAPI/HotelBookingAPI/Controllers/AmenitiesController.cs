using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class AmenitiesController : ControllerBase
{
    private readonly IAmenityService _service;

    public AmenitiesController(IAmenityService service)
    {
        _service = service;
    }

    // GET: api/Amenities?keyword=wifi
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AmenityDto>>> GetAll([FromQuery] string? keyword)
    {
        return Ok(await _service.GetAllAsync(keyword));
    }

    // GET: api/Amenities/5
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<AmenityDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    // POST: api/Amenities
    [HttpPost]
    public async Task<ActionResult<AmenityDto>> Create(CreateAmenityDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Amenities/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateAmenityDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    // DELETE: api/Amenities/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}