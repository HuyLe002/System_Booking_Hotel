using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface IBookingService
{
    Task<PagedResult<BookingDto>> GetPagedAsync(BookingQuery query, CurrentUser user);
    Task<BookingDto> GetByIdAsync(int id, CurrentUser user);
    Task<List<RoomDto>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId);
    Task<BookingDto> CreateAsync(CreateBookingDto dto, CurrentUser user);
    Task UpdateDatesAsync(int id, UpdateBookingDatesDto dto, CurrentUser user);
    Task UpdateServicesAsync(int id, UpdateBookingServicesDto dto, CurrentUser user);
    Task ConfirmAsync(int id);
    Task CheckInAsync(int id);
    Task CheckOutAsync(int id);
    Task CancelAsync(int id, CurrentUser user);
    Task DeleteAsync(int id);
}