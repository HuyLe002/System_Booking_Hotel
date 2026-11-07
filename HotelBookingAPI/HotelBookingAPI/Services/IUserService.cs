using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto> CreateStaffAsync(CreateStaffDto dto);
    Task DeleteStaffAsync(int id);
}