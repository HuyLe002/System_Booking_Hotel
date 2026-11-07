using System.Security.Claims;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    // Id tài khoản đang đăng nhập, lấy từ token
    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // POST: api/Auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var customerId = await _service.RegisterAsync(dto);
        return Ok(new { message = "Đăng ký thành công", customerId });
    }

    // POST: api/Auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        return Ok(await _service.LoginAsync(dto));
    }

    // POST: api/Auth/refresh  (đổi refresh token lấy cặp token mới)
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenDto dto)
    {
        return Ok(await _service.RefreshAsync(dto));
    }

    // POST: api/Auth/logout  (thu hồi một refresh token)
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenDto dto)
    {
        await _service.LogoutAsync(dto);
        return NoContent();
    }

    // POST: api/Auth/logout-all  (đăng xuất mọi thiết bị)
    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        await _service.LogoutAllAsync(CurrentUserId);
        return NoContent();
    }

    // POST: api/Auth/change-password
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        await _service.ChangePasswordAsync(CurrentUserId, dto);
        return NoContent();
    }

    // GET: api/Auth/me  (thông tin trong token)
    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            id = CurrentUserId,
            username = User.Identity?.Name,
            role = User.FindFirst(ClaimTypes.Role)?.Value,
            customerId = User.FindFirst("customerId")?.Value
        });
    }
}