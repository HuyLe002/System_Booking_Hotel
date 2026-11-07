namespace HotelBookingAPI.Models;

public class PaymentClaim
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Status { get; set; } = PaymentClaimStatuses.Pending;
    public DateTime CustomerConfirmedAt { get; set; }
    public DateTime? CashierReviewedAt { get; set; }
    public int? CashierUserId { get; set; }
    public string? CashierNote { get; set; }

    public Booking? Booking { get; set; }
}

public static class PaymentClaimStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}
