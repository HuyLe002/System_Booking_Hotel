namespace HotelBookingAPI.Common;

public static class AuditActions
{
    public const string Create = "Create";
    public const string UpdateDates = "UpdateDates";
    public const string UpdateServices = "UpdateServices";
    public const string Confirm = "Confirm";
    public const string CheckIn = "CheckIn";
    public const string CheckOut = "CheckOut";
    public const string Cancel = "Cancel";
    public const string Delete = "Delete";
    public const string PaymentClaimSubmitted = "PaymentClaimSubmitted";
    public const string PaymentClaimConfirmed = "PaymentClaimConfirmed";
    public const string PaymentClaimRejected = "PaymentClaimRejected";
    public const string RefundRecorded = "RefundRecorded";
}