namespace HotelBookingAPI.Common;

// Một nơi duy nhất định nghĩa tên trạng thái, tránh gõ chuỗi rải rác
public static class BookingStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string CheckedIn = "CheckedIn";
    public const string CheckedOut = "CheckedOut";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = { Pending, Confirmed, CheckedIn, CheckedOut, Cancelled };

    // Các trạng thái được tính vào doanh thu
    public static readonly string[] Revenue = { Confirmed, CheckedIn, CheckedOut };
}