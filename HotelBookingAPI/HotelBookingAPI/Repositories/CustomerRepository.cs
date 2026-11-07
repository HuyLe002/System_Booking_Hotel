using HotelBookingAPI.Common;
using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly HotelDbContext _context;

    public CustomerRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<(List<CustomerWithBookingCount> Items, int TotalItems)> GetPagedAsync(
        CustomerQuery query, string sortBy, bool desc)
    {
        // 1. Lọc (global filter đã tự loại khách đã xóa mềm)
        var customers = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            customers = customers.Where(x =>
                x.FullName.Contains(query.Keyword) ||
                x.Email.Contains(query.Keyword) ||
                (x.Phone != null && x.Phone.Contains(query.Keyword)));
        }

        // 2. Đếm tổng (trước khi phân trang)
        var totalItems = await customers.CountAsync();

        // 3. Gắn số booking vào từng dòng (EF dịch thành COUNT trong SQL)
        var rows = customers.Select(x => new { Customer = x, TotalBookings = x.Bookings.Count });

        // 4. Sắp xếp (thêm khóa phụ Id để thứ tự luôn xác định)
        var sorted = (sortBy, desc) switch
        {
            ("email", false) => rows.OrderBy(x => x.Customer.Email).ThenBy(x => x.Customer.Id),
            ("email", true) => rows.OrderByDescending(x => x.Customer.Email).ThenBy(x => x.Customer.Id),
            ("bookings", false) => rows.OrderBy(x => x.TotalBookings).ThenBy(x => x.Customer.Id),
            ("bookings", true) => rows.OrderByDescending(x => x.TotalBookings).ThenBy(x => x.Customer.Id),
            (_, true) => rows.OrderByDescending(x => x.Customer.FullName).ThenBy(x => x.Customer.Id),
            _ => rows.OrderBy(x => x.Customer.FullName).ThenBy(x => x.Customer.Id)
        };

        // 5. Phân trang
        var data = await sorted
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var items = data
            .Select(d => new CustomerWithBookingCount(d.Customer, d.TotalBookings))
            .ToList();

        return (items, totalItems);
    }

    public async Task<CustomerWithBookingCount?> GetByIdWithCountAsync(int id)
    {
        var data = await _context.Customers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { Customer = x, TotalBookings = x.Bookings.Count })
            .FirstOrDefaultAsync();

        return data == null
            ? null
            : new CustomerWithBookingCount(data.Customer, data.TotalBookings);
    }

    // Không dùng FindAsync vì FindAsync bỏ qua global query filter
    public async Task<Customer?> GetForUpdateAsync(int id) =>
        await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);

    // IgnoreQueryFilters: cố ý nhìn thấy cả dòng đã xóa mềm
    public async Task<List<Customer>> GetDeletedAsync() =>
        await _context.Customers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAt)
            .ToListAsync();

    public async Task<Customer?> GetDeletedForUpdateAsync(int id) =>
        await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted);

    // excludeId dùng khi cập nhật: bỏ qua chính khách hàng đang sửa
    // (chỉ xét khách chưa xóa nhờ global filter)
    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null) =>
        await _context.Customers.AnyAsync(x =>
            x.Email == email && (excludeId == null || x.Id != excludeId));

    public async Task<bool> HasBookingsAsync(int customerId) =>
        await _context.Bookings.AnyAsync(x => x.CustomerId == customerId);

    public async Task AddAsync(Customer entity) =>
        await _context.Customers.AddAsync(entity);

    // Xóa mềm: chỉ đánh dấu, không xóa khỏi database
    public void Remove(Customer entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
    }

    public void Restore(Customer entity)
    {
        entity.IsDeleted = false;
        entity.DeletedAt = null;
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}