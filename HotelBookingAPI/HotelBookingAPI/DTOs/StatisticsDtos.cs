namespace HotelBookingAPI.DTOs;

public class SummaryDto
{
    public int TotalRooms { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; } // chỉ tính booking Confirmed
}

public class MonthlyRevenueDto
{
    public int Month { get; set; }
    public int TotalBookings { get; set; }
    public decimal Revenue { get; set; }
}

public class BookingStatusStatDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TopRoomDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public int TotalNights { get; set; }
    public decimal Revenue { get; set; }
}

public class TopCustomerDto
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public decimal TotalSpent { get; set; }
}