using System.Text.Json;
using HotelBookingAPI.Common;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi chưa được xử lý tại {Path}", context.Request.Path);
            await WriteErrorAsync(context, ex);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        // DbUpdateConcurrencyException kế thừa DbUpdateException nên phải đặt trước
        var (status, message) = ex switch
        {
            DbUpdateConcurrencyException => (409, "Dữ liệu đã bị thay đổi bởi người khác, vui lòng tải lại"),
            DbUpdateException => (409, "Không thể lưu dữ liệu, có thể vi phạm ràng buộc (trùng dữ liệu hoặc khóa ngoại)"),
            KeyNotFoundException => (404, ex.Message),
            InvalidCredentialsException => (401, ex.Message),
            UnauthorizedAccessException => (403, ex.Message),
            ArgumentException => (400, ex.Message),
            _ => (500, "Đã xảy ra lỗi hệ thống")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var body = JsonSerializer.Serialize(new { status, message });
        await context.Response.WriteAsync(body);
    }
}