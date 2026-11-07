using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class ExtraServicesController : ControllerBase
{
    private readonly IExtraServiceService _service;

    public ExtraServicesController(IExtraServiceService service)
    {
        _service = service;
    }

    // GET: api/ExtraServices?activeOnly=true
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExtraServiceDto>>> GetAll([FromQuery] bool activeOnly = false)
    {
        return Ok(await _service.GetAllAsync(activeOnly));
    }

    // GET: api/ExtraServices/1
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExtraServiceDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    // POST: api/ExtraServices  (Admin)
    [HttpPost]
    public async Task<ActionResult<ExtraServiceDto>> Create(CreateExtraServiceDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/ExtraServices/1  (Admin)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateExtraServiceDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    // DELETE: api/ExtraServices/1  (Admin, chỉ khi chưa dùng trong booking nào)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}