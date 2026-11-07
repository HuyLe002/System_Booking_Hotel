using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface IRoomTypeService
{
    Task<List<RoomTypeDto>> GetAllAsync();
    Task<RoomTypeDto> GetByIdAsync(int id);
    Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto dto);
    Task UpdateAsync(int id, UpdateRoomTypeDto dto);
    Task DeleteAsync(int id);
}