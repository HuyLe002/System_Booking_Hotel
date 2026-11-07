using HotelBookingAPI.Models;
using HotelBookingAPI.Services;

namespace HotelBookingAPI.Tests;

public class PaymentRulesTests
{
    [Fact]
    public void NetPaidSubtractsRefundsFromPayments()
    {
        var payments = new[]
        {
            new Payment { Amount = 500_000, Type = PaymentRules.TypePayment },
            new Payment { Amount = 100_000, Type = PaymentRules.TypeRefund },
            new Payment { Amount = 250_000, Type = PaymentRules.TypePayment }
        };

        Assert.Equal(650_000, PaymentRules.NetPaid(payments));
    }

    [Fact]
    public void RequiredDepositRoundsToWholeDongAwayFromZero()
    {
        Assert.Equal(100, PaymentRules.RequiredDeposit(333.33m));
    }

    [Theory]
    [InlineData(0, 0, "Unpaid")]
    [InlineData(100, 100, "Refunded")]
    [InlineData(400, 0, "PartiallyPaid")]
    [InlineData(1000, 0, "Paid")]
    [InlineData(1000, 400, "Retained")]
    public void GetStatusReflectsPaymentsRefundsAndCancellation(
        decimal paid, decimal refunded, string expected)
    {
        var status = PaymentRules.GetStatus(
            totalPrice: 1_000,
            paid,
            refunded,
            bookingStatus: expected == "Retained" ? "Cancelled" : "Confirmed");

        Assert.Equal(expected, status);
    }

    [Fact]
    public void CancelledBookingHasNoOutstandingBalance()
    {
        Assert.Equal(0, PaymentRules.BalanceDue(1_000, 200, "Cancelled"));
    }

    [Theory]
    [InlineData(7, 100, 1000)]
    [InlineData(3, 50, 500)]
    [InlineData(1, 20, 200)]
    [InlineData(0, 0, 0)]
    public void RefundPolicyUsesExpectedCancellationTier(
        int daysBeforeCheckIn, decimal expectedPercent, decimal expectedAmount)
    {
        var result = RefundPolicy.Calculate(1_000, daysBeforeCheckIn);

        Assert.Equal(expectedPercent, result.Percent);
        Assert.Equal(expectedAmount, result.Amount);
    }
}
