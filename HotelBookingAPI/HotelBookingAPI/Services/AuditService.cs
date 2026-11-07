using System.Security.Claims;
using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public interface IAuditService
{
    // Chỉ chuẩn bị dòng log; caller gọi SaveChanges để lưu cùng thay đổi nghiệp vụ
    void Log(string action, string entityType, int entityId, string? details = null);
    Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQuery query);
}

public class AuditService : IAuditService
{
    private readonly IAuditRepository _repo;
    private readonly IHttpContextAccessor _http;

    public AuditService(IAuditRepository repo, IHttpContextAccessor http)
    {
        _repo = repo;
        _http = http;
    }

    public void Log(string action, string entityType, int entityId, string? details = null)
    {
        var user = _http.HttpContext?.User;

        // Thử lần lượt các tên claim phổ biến của JWT
        var userIdText = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user?.FindFirst("sub")?.Value;

        var userName = user?.Identity?.Name
            ?? user?.FindFirst(ClaimTypes.Name)?.Value
            ?? user?.FindFirst("unique_name")?.Value
            ?? user?.FindFirst("name")?.Value
            ?? "system";

        var role = user?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        _repo.Add(new AuditLog
        {
            CreatedAt = DateTime.UtcNow,
            UserId = int.TryParse(userIdText, out var uid) ? uid : null,
            UserName = userName,
            Role = role,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details is { Length: > 500 } ? details[..500] : details
        });
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQuery query)
    {
        if (query.From.HasValue && query.To.HasValue && query.To < query.From)
            throw new ArgumentException("to phải sau hoặc bằng from");

        // Mặc định mới nhất trước
        var desc = query.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

        var (items, totalItems) = await _repo.GetPagedAsync(query, desc);

        return new PagedResult<AuditLogDto>
        {
            Items = items.Select(a => new AuditLogDto
            {
                Id = a.Id,
                CreatedAt = a.CreatedAt,
                UserId = a.UserId,
                UserName = a.UserName,
                Role = a.Role,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Details = a.Details
            }).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems
        };
    }
}