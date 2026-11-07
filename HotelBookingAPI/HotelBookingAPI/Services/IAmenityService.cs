using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface IAmenityService
{
    Task<List<AmenityDto>> GetAllAsync(string? keyword);
    Task<AmenityDto> GetByIdAsync(int id);
    Task<AmenityDto> CreateAsync(CreateAmenityDto dto);
    Task UpdateAsync(int id, UpdateAmenityDto dto);
    Task DeleteAsync(int id);
}