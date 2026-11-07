using HotelBookingAPI.Models;
using HotelBookingAPI.Services;

namespace HotelBookingAPI.Tests;

public class PriceCalculatorTests
{
    [Fact]
    public void CalculatesEveryNightAndAppliesWeekendRule()
    {
        var rules = new[]
        {
            new PricingRule
            {
                Id = 1,
                RuleType = "Weekend",
                Percent = 20,
                IsActive = true
            }
        };

        var result = PriceCalculator.Calculate(
            new DateTime(2026, 11, 6),
            new DateTime(2026, 11, 8),
            500_000,
            rules);

        Assert.Equal(2, result.Nights.Count);
        Assert.Equal(new DateTime(2026, 11, 6), result.Nights[0].Date);
        Assert.Equal(600_000, result.Nights[0].Price);
        Assert.Equal(600_000, result.Nights[1].Price);
        Assert.Equal(1_200_000, result.Total);
    }

    [Fact]
    public void HighestPriorityApplicableRuleWinsWithoutStacking()
    {
        var rules = new[]
        {
            new PricingRule
            {
                Id = 1,
                RuleType = "Weekend",
                Percent = 20,
                Priority = 1,
                IsActive = true
            },
            new PricingRule
            {
                Id = 2,
                Name = "Season",
                RuleType = "Season",
                StartDate = new DateTime(2026, 11, 7),
                EndDate = new DateTime(2026, 11, 7),
                Percent = 50,
                Priority = 2,
                IsActive = true
            }
        };

        var result = PriceCalculator.Calculate(
            new DateTime(2026, 11, 7),
            new DateTime(2026, 11, 8),
            500_000,
            rules);

        Assert.Equal(750_000, result.Total);
        Assert.Equal("Season", result.Nights[0].RuleName);
    }

    [Fact]
    public void RoundsNightPriceToWholeDongAwayFromZero()
    {
        var rules = new[]
        {
            new PricingRule
            {
                Id = 1,
                RuleType = "Weekend",
                Percent = 10,
                IsActive = true
            }
        };

        var result = PriceCalculator.Calculate(
            new DateTime(2026, 11, 6),
            new DateTime(2026, 11, 7),
            500_001,
            rules);

        Assert.Equal(550_001, result.Total);
    }
}
