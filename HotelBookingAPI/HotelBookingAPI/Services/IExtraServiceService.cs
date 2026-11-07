using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface IExtraServiceService
{
    Task<List<ExtraServiceDto>> GetAllAsync(bool activeOnly);
    Task<ExtraServiceDto> GetByIdAsync(int id);
    Task<ExtraServiceDto> CreateAsync(CreateExtraServiceDto dto);
    Task UpdateAsync(int id, UpdateExtraServiceDto dto);
    Task DeleteAsync(int id);
}