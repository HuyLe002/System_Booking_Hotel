namespace HotelBookingAPI.Services;

public record RefundResult(decimal Percent, decimal Amount);

// Chính sách hoàn tiền khi hủy, không dùng database nên dễ viết unit test
public static class RefundPolicy
{
    // (số ngày tối thiểu trước ngày nhận phòng, % hoàn), xếp từ lớn xuống nhỏ
    private static readonly (int MinDays, decimal Percent)[] Tiers =
    {
        (7, 100m),
        (3, 50m),
        (1, 20m)
    };

    public static RefundResult Calculate(decimal netPaid, int daysBeforeCheckIn)
    {
        if (netPaid <= 0)
            return new RefundResult(0, 0);

        // Mốc đầu tiên thỏa điều kiện chính là mốc cao nhất; không mốc nào thỏa thì 0%
        var percent = Tiers
            .Where(t => daysBeforeCheckIn >= t.MinDays)
            .Select(t => t.Percent)
            .FirstOrDefault();

        var amount = Math.Round(netPaid * percent / 100m, 0, MidpointRounding.AwayFromZero);

        return new RefundResult(percent, amount);
    }
}