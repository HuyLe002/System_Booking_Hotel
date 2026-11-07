namespace HotelBookingAPI.Repositories;

// Các dòng kết quả thô do Repository trả về, Service sẽ đổi sang DTO
public record SummaryData(int TotalRooms, int TotalCustomers, int TotalBookings, decimal TotalRevenue);

public record MonthlyRevenueRow(int Month, int TotalBookings, decimal Revenue);

public record BookingStatusRow(string Status, int Count);

public record TopRoomRow(int RoomId, string RoomNumber, string RoomTypeName, int TotalBookings, int TotalNights, decimal Revenue);

public record TopCustomerRow(int CustomerId, string FullName, int TotalBookings, decimal TotalSpent);