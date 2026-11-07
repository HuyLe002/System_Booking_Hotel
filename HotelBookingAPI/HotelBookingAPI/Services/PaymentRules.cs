using HotelBookingAPI.Models;

namespace HotelBookingAPI.Services;

// Không dùng database: nhận danh sách giao dịch rồi tính ra số liệu
public static class PaymentRules
{
    public const string TypePayment = "Payment";
    public const string TypeRefund = "Refund";

    // Phải thu tối thiểu 30% tổng tiền thì mới được nhận phòng
    public const decimal DepositPercent = 30m;

    public static decimal TotalPaid(IEnumerable<Payment> payments) =>
        payments.Where(p => p.Type == TypePayment).Sum(p => p.Amount);

    public static decimal TotalRefunded(IEnumerable<Payment> payments) =>
        payments.Where(p => p.Type == TypeRefund).Sum(p => p.Amount);

    // Thực thu = đã thu - đã hoàn
    public static decimal NetPaid(IEnumerable<Payment> payments) =>
        TotalPaid(payments) - TotalRefunded(payments);

    public static decimal RequiredDeposit(decimal totalPrice) =>
        Math.Round(totalPrice * DepositPercent / 100m, 0, MidpointRounding.AwayFromZero);

    public static string GetStatus(decimal totalPrice, decimal paid, decimal refunded, string bookingStatus)
    {
        var net = paid - refunded;

        if (paid == 0)
            return "Unpaid";

        if (net <= 0)
            return "Refunded";

        // Đã hủy mà vẫn còn tiền khách đã trả: khách mất khoản này theo chính sách
        if (bookingStatus == "Cancelled")
            return "Retained";

        return net >= totalPrice ? "Paid" : "PartiallyPaid";
    }

    // Booking đã hủy thì không còn gì phải trả
    public static decimal BalanceDue(decimal totalPrice, decimal netPaid, string bookingStatus) =>
        bookingStatus == "Cancelled" ? 0 : Math.Max(0, totalPrice - netPaid);
}