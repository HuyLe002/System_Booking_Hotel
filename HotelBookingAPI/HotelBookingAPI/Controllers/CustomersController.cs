using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

// Cả controller cho Staff hoặc Admin. Method có thêm [Authorize(Roles = Admin)] thì phải thỏa CẢ HAI,
// tức là chỉ còn Admin
[Authorize(Policy = Policies.StaffOrAdmin)]
[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomersController(ICustomerService service)
    {
        _service = service;
    }

    // GET: api/Customers?keyword=nguyen&page=1&pageSize=5&sortBy=bookings&sortDir=desc
    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll([FromQuery] CustomerQuery query)
    {
        return Ok(await _service.GetPagedAsync(query));
    }

    // GET: api/Customers/deleted  (chỉ Admin)
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("deleted")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetDeleted()
    {
        return Ok(await _service.GetDeletedAsync());
    }

    // GET: api/Customers/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    // POST: api/Customers
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Customers/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCustomerDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    // PUT: api/Customers/5/restore  (chỉ Admin)
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        await _service.RestoreAsync(id);
        return NoContent();
    }

    // DELETE: api/Customers/5  (chỉ Admin, xóa mềm)
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}