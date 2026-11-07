using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _service;

    public StatisticsController(IStatisticsService service)
    {
        _service = service;
    }

    // GET: api/Statistics/summary
    [HttpGet("summary")]
    public async Task<ActionResult<SummaryDto>> GetSummary()
    {
        return Ok(await _service.GetSummaryAsync());
    }

    // GET: api/Statistics/revenue-by-month?year=2026
    [HttpGet("revenue-by-month")]
    public async Task<ActionResult<IEnumerable<MonthlyRevenueDto>>> GetRevenueByMonth([FromQuery] int year)
    {
        return Ok(await _service.GetRevenueByMonthAsync(year));
    }

    // GET: api/Statistics/bookings-by-status
    [HttpGet("bookings-by-status")]
    public async Task<ActionResult<IEnumerable<BookingStatusStatDto>>> GetBookingsByStatus()
    {
        return Ok(await _service.GetBookingsByStatusAsync());
    }

    // GET: api/Statistics/top-rooms?top=3
    [HttpGet("top-rooms")]
    public async Task<ActionResult<IEnumerable<TopRoomDto>>> GetTopRooms([FromQuery] int top = 5)
    {
        return Ok(await _service.GetTopRoomsAsync(top));
    }

    // GET: api/Statistics/top-customers?top=3
    [HttpGet("top-customers")]
    public async Task<ActionResult<IEnumerable<TopCustomerDto>>> GetTopCustomers([FromQuery] int top = 5)
    {
        return Ok(await _service.GetTopCustomersAsync(top));
    }
}