using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerQuery query);
    Task<CustomerDto> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task UpdateAsync(int id, UpdateCustomerDto dto);
    Task DeleteAsync(int id);
    Task<List<CustomerDto>> GetDeletedAsync();
    Task RestoreAsync(int id);
}