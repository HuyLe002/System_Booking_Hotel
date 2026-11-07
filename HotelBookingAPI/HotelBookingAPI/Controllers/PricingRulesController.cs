using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class PricingRulesController : ControllerBase
{
    private readonly IPricingRuleService _service;

    public PricingRulesController(IPricingRuleService service)
    {
        _service = service;
    }

    // GET: api/PricingRules?activeOnly=true
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PricingRuleDto>>> GetAll([FromQuery] bool activeOnly = false)
    {
        return Ok(await _service.GetAllAsync(activeOnly));
    }

    // GET: api/PricingRules/quote?roomId=1&checkIn=2027-03-10&checkOut=2027-03-13  (ai cũng xem được)
    [AllowAnonymous]
    [HttpGet("quote")]
    public async Task<ActionResult<QuoteDto>> GetQuote(
        [FromQuery] int roomId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        return Ok(await _service.GetQuoteAsync(roomId, checkIn, checkOut));
    }

    // GET: api/PricingRules/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PricingRuleDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    // POST: api/PricingRules
    [HttpPost]
    public async Task<ActionResult<PricingRuleDto>> Create(CreatePricingRuleDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/PricingRules/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdatePricingRuleDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    // DELETE: api/PricingRules/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}