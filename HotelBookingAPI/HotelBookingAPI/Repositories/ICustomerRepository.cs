using HotelBookingAPI.Common;
using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface ICustomerRepository
{
    Task<(List<CustomerWithBookingCount> Items, int TotalItems)> GetPagedAsync(CustomerQuery query, string sortBy, bool desc);
    Task<CustomerWithBookingCount?> GetByIdWithCountAsync(int id);  // chỉ đọc
    Task<Customer?> GetForUpdateAsync(int id);                       // có tracking
    Task<List<Customer>> GetDeletedAsync();                          // khách đã xóa mềm
    Task<Customer?> GetDeletedForUpdateAsync(int id);                // khách đã xóa mềm, có tracking
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<bool> HasBookingsAsync(int customerId);
    Task AddAsync(Customer entity);
    void Remove(Customer entity);                                    // xóa mềm
    void Restore(Customer entity);                                   // khôi phục
    Task SaveChangesAsync();
}