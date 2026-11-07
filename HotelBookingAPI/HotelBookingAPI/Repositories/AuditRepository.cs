using HotelBookingAPI.Common;
using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public interface IAuditRepository
{
    // Chỉ đưa vào context, KHÔNG SaveChanges: để cùng transaction với thay đổi nghiệp vụ
    void Add(AuditLog log);
    Task<(List<AuditLog> Items, int TotalItems)> GetPagedAsync(AuditLogQuery query, bool desc);
}

public class AuditRepository : IAuditRepository
{
    private readonly HotelDbContext _context;

    public AuditRepository(HotelDbContext context)
    {
        _context = context;
    }

    public void Add(AuditLog log) => _context.AuditLogs.Add(log);

    public async Task<(List<AuditLog> Items, int TotalItems)> GetPagedAsync(AuditLogQuery query, bool desc)
    {
        var q = _context.AuditLogs.AsNoTracking().AsQueryable();

        // 1. Lọc
        if (!string.IsNullOrWhiteSpace(query.EntityType))
            q = q.Where(a => a.EntityType == query.EntityType);

        if (query.EntityId.HasValue)
            q = q.Where(a => a.EntityId == query.EntityId);

        if (!string.IsNullOrWhiteSpace(query.UserName))
            q = q.Where(a => a.UserName.Contains(query.UserName));

        if (!string.IsNullOrWhiteSpace(query.Action))
            q = q.Where(a => a.Action == query.Action);

        if (query.From.HasValue)
            q = q.Where(a => a.CreatedAt >= query.From.Value.Date);

        if (query.To.HasValue)
            q = q.Where(a => a.CreatedAt < query.To.Value.Date.AddDays(1));

        // 2. Đếm trước khi phân trang
        var totalItems = await q.CountAsync();

        // 3. Sắp xếp (thêm Id để thứ tự ổn định khi trùng thời điểm)
        q = desc
            ? q.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id)
            : q.OrderBy(a => a.CreatedAt).ThenBy(a => a.Id);

        // 4. Phân trang
        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalItems);
    }
}