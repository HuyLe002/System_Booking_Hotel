using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class CustomerService : ICustomerService
{
    private static readonly string[] AllowedSortColumns = { "fullname", "email", "bookings" };

    private readonly ICustomerRepository _repo;

    public CustomerService(ICustomerRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerQuery query)
    {
        var sortBy = (query.SortBy ?? "fullname").ToLower();
        if (!AllowedSortColumns.Contains(sortBy))
            throw new ArgumentException("sortBy chỉ nhận: fullName, email, bookings");

        var desc = query.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

        var (items, totalItems) = await _repo.GetPagedAsync(query, sortBy, desc);

        return new PagedResult<CustomerDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<CustomerDto> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdWithCountAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy khách hàng có Id = {id}");

        return ToDto(item);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var email = dto.Email.Trim();

        if (await _repo.EmailExistsAsync(email))
            throw new ArgumentException("Email đã được sử dụng");

        var customer = new Customer
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            Phone = dto.Phone
        };

        await _repo.AddAsync(customer);
        await _repo.SaveChangesAsync();

        // Khách vừa tạo chưa có booking nào
        return ToDto(new CustomerWithBookingCount(customer, 0));
    }

    public async Task UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy khách hàng có Id = {id}");

        var email = dto.Email.Trim();

        if (await _repo.EmailExistsAsync(email, excludeId: id))
            throw new ArgumentException("Email đã được sử dụng");

        customer.FullName = dto.FullName.Trim();
        customer.Email = email;
        customer.Phone = dto.Phone;

        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy khách hàng có Id = {id}");

        if (await _repo.HasBookingsAsync(id))
            throw new ArgumentException("Không thể xóa vì khách hàng đã có lịch đặt phòng");

        _repo.Remove(customer);   // xóa mềm
        await _repo.SaveChangesAsync();
    }

    public async Task<List<CustomerDto>> GetDeletedAsync()
    {
        var items = await _repo.GetDeletedAsync();
        return items.Select(c => ToDto(new CustomerWithBookingCount(c, 0))).ToList();
    }

    public async Task RestoreAsync(int id)
    {
        var customer = await _repo.GetDeletedForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy khách hàng đã xóa có Id = {id}");

        // Trong lúc bị xóa, email có thể đã được người khác dùng
        if (await _repo.EmailExistsAsync(customer.Email))
            throw new ArgumentException("Email này đã được khách hàng khác sử dụng, không thể khôi phục");

        _repo.Restore(customer);
        await _repo.SaveChangesAsync();
    }

    private static CustomerDto ToDto(CustomerWithBookingCount x) => new()
    {
        Id = x.Customer.Id,
        FullName = x.Customer.FullName,
        Email = x.Customer.Email,
        Phone = x.Customer.Phone,
        TotalBookings = x.TotalBookings
    };
}