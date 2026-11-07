using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _service;

    public AuditLogsController(IAuditService service)
    {
        _service = service;
    }

    // GET: api/AuditLogs?entityType=Booking&entityId=5&userName=staff&action=CheckIn&from=2026-11-01&to=2026-11-30&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAll([FromQuery] AuditLogQuery query)
    {
        return Ok(await _service.GetPagedAsync(query));
    }
}