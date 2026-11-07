using static HotelBookingAPI.Common.BookingStatuses;

namespace HotelBookingAPI.Services;

// Bảng chuyển trạng thái hợp lệ nằm ở một chỗ, không rải rác bằng các câu if
public static class BookingStateMachine
{
    private static readonly Dictionary<string, string[]> Allowed = new()
    {
        [Pending] = new[] { Confirmed, Cancelled },
        [Confirmed] = new[] { CheckedIn, Cancelled },
        [CheckedIn] = new[] { CheckedOut },
        [CheckedOut] = Array.Empty<string>(),   // trạng thái cuối
        [Cancelled] = Array.Empty<string>()     // trạng thái cuối
    };

    public static bool CanTransition(string from, string to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);

    // Không hợp lệ thì ném ArgumentException (middleware đổi thành 400)
    public static void EnsureTransition(string from, string to)
    {
        if (CanTransition(from, to))
            return;

        throw new ArgumentException(to switch
        {
            Confirmed => "Chỉ xác nhận được booking đang ở trạng thái Pending",
            CheckedIn => "Chỉ nhận phòng được booking đã xác nhận (Confirmed)",
            CheckedOut => "Chỉ trả phòng được booking đang ở (CheckedIn)",
            Cancelled when from == Cancelled => "Booking đã được hủy trước đó",
            Cancelled => "Không thể hủy booking đã nhận phòng hoặc đã trả phòng",
            _ => $"Không thể chuyển booking từ {from} sang {to}"
        });
    }
}