namespace HotelBookingAPI.Common;

// Thông tin người đang gọi API, Controller lấy từ token rồi truyền xuống Service
public record CurrentUser(bool IsAdmin, int? CustomerId);