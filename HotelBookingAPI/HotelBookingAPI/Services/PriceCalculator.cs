using HotelBookingAPI.Models;

namespace HotelBookingAPI.Services;

// Giá của một đêm
public record NightPrice(DateTime Date, decimal BasePrice, decimal Price, string? RuleName, decimal? Percent);

// Kết quả tính cho cả kỳ nghỉ
public record PriceBreakdown(List<NightPrice> Nights, decimal Total);

// Không dùng database: nhận ngày, giá cơ bản và danh sách quy tắc (đã lọc theo loại phòng và đang bật)
public static class PriceCalculator
{
    public const int MaxNights = 90;

    public static PriceBreakdown Calculate(
        DateTime checkIn, DateTime checkOut, decimal basePrice, IEnumerable<PricingRule> rules)
    {
        var ruleList = rules.ToList();
        var nights = new List<NightPrice>();

        // Mỗi đêm là một ngày từ checkIn đến trước checkOut
        for (var date = checkIn.Date; date < checkOut.Date; date = date.AddDays(1))
        {
            // Không cộng dồn: chỉ lấy một quy tắc, ưu tiên Priority cao, bằng nhau thì Id nhỏ
            var rule = ruleList
                .Where(r => Applies(r, date))
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.Id)
                .FirstOrDefault();

            var price = rule == null
                ? basePrice
                : Math.Round(basePrice * (1 + rule.Percent / 100m), 0, MidpointRounding.AwayFromZero);

            nights.Add(new NightPrice(date, basePrice, price, rule?.Name, rule?.Percent));
        }

        return new PriceBreakdown(nights, nights.Sum(n => n.Price));
    }

    private static bool Applies(PricingRule rule, DateTime night) => rule.RuleType switch
    {
        // Đêm thứ Sáu và thứ Bảy
        "Weekend" => night.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday,

        "Season" => rule.StartDate.HasValue && rule.EndDate.HasValue
            && night.Date >= rule.StartDate.Value.Date
            && night.Date <= rule.EndDate.Value.Date,

        _ => false
    };
}