using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs;

// Dùng cho cả ghi nhận thu tiền lẫn hoàn tiền thủ công
public class CreatePaymentDto
{
    [Range(1, 1000000000, ErrorMessage = "Số tiền phải từ 1 đến 1.000.000.000")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
    [RegularExpression("^(Cash|BankTransfer|Card)$", ErrorMessage = "Method chỉ nhận: Cash, BankTransfer hoặc Card")]
    public string Method { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    public string? Note { get; set; }
}

public class CreatePaymentClaimDto
{
    [Range(1, 1000000000, ErrorMessage = "Số tiền phải từ 1 đến 1.000.000.000")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
    [RegularExpression("^(Cash|BankTransfer|Card)$", ErrorMessage = "Method chỉ nhận: Cash, BankTransfer hoặc Card")]
    public string Method { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    public string? Note { get; set; }
}

public class ReviewPaymentClaimDto
{
    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    public string? Note { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentClaimDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CustomerConfirmedAt { get; set; }
    public DateTime? CashierReviewedAt { get; set; }
    public int? CashierUserId { get; set; }
    public string? CashierNote { get; set; }
}

public class PaymentClaimQueueItemDto : PaymentClaimDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal BookingTotal { get; set; }
    public decimal BalanceDue { get; set; }
}

// Tổng hợp sổ thanh toán của một booking
public class BookingPaymentsDto
{
    public int BookingId { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalRefunded { get; set; }
    public decimal NetPaid => TotalPaid - TotalRefunded;
    public decimal BalanceDue { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public List<PaymentDto> Items { get; set; } = new();
    public List<PaymentClaimDto> PaymentClaims { get; set; } = new();
}