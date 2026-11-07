using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface IReviewService
{
    Task<RoomReviewsDto> GetByRoomAsync(int roomId, PagingQuery query);
    Task<ReviewDto> GetByIdAsync(int id);
    Task<ReviewDto> CreateAsync(CreateReviewDto dto, CurrentUser user);
    Task UpdateAsync(int id, UpdateReviewDto dto, CurrentUser user);
    Task DeleteAsync(int id, CurrentUser user);
}