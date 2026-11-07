using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _service;

    public CouponsController(ICouponService service)
    {
        _service = service;
    }

    // GET: api/Coupons?activeOnly=true
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CouponDto>>> GetAll([FromQuery] bool activeOnly = false)
    {
        return Ok(await _service.GetAllAsync(activeOnly));
    }

    // GET: api/Coupons/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CouponDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    // POST: api/Coupons
    [HttpPost]
    public async Task<ActionResult<CouponDto>> Create(CreateCouponDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Coupons/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCouponDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    // DELETE: api/Coupons/1  (chỉ khi chưa dùng trong booking nào)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}