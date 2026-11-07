using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface IRoomService
{
    Task<PagedResult<RoomDto>> GetPagedAsync(RoomQuery query);
    Task<RoomDto> GetByIdAsync(int id);
    Task<RoomDto> CreateAsync(CreateRoomDto dto);
    Task UpdateAsync(int id, UpdateRoomDto dto);
    Task DeleteAsync(int id);
}