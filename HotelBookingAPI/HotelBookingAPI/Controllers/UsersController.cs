using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    // POST: api/Users/staff
    [HttpPost("staff")]
    public async Task<ActionResult<UserDto>> CreateStaff(CreateStaffDto dto)
    {
        var created = await _service.CreateStaffAsync(dto);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    // DELETE: api/Users/staff/5
    [HttpDelete("staff/{id:int}")]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        await _service.DeleteStaffAsync(id);
        return NoContent();
    }
}