using HotelBookingAPI.Models;

namespace HotelBookingAPI.Services;

// Không dùng database, chỉ nhận dữ liệu vào và trả kết quả: dễ đọc, dễ viết unit test
public static class CouponRules
{
    // Kiểm tra mã và trả về số tiền được giảm. Sai luật thì ném ArgumentException (thành 400)
    public static decimal Evaluate(Coupon coupon, decimal subtotal, DateTime today)
    {
        if (!coupon.IsActive)
            throw new ArgumentException("Mã giảm giá đã bị tắt");

        if (today < coupon.ValidFrom.Date)
            throw new ArgumentException("Mã giảm giá chưa đến thời gian áp dụng");

        if (today > coupon.ValidTo.Date)
            throw new ArgumentException("Mã giảm giá đã hết hạn");

        if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses.Value)
            throw new ArgumentException("Mã giảm giá đã hết lượt sử dụng");

        if (subtotal < coupon.MinBookingAmount)
            throw new ArgumentException(
                $"Đơn hàng tối thiểu {coupon.MinBookingAmount:N0} đ để dùng mã này");

        var discount = coupon.DiscountType == "Percent"
            ? subtotal * coupon.Value / 100m
            : coupon.Value;

        // Trần giảm tối đa chỉ áp dụng cho kiểu phần trăm
        if (coupon.DiscountType == "Percent" && coupon.MaxDiscount.HasValue)
            discount = Math.Min(discount, coupon.MaxDiscount.Value);

        // Làm tròn đến đồng, và không bao giờ giảm quá tổng tạm tính
        discount = Math.Round(discount, 0, MidpointRounding.AwayFromZero);
        return Math.Min(discount, subtotal);
    }
}