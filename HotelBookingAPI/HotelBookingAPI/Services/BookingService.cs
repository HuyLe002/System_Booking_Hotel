using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class BookingService : IBookingService
{
    private static readonly string[] AllowedSortColumns = { "checkin", "checkout", "price", "status" };

    private readonly IBookingRepository _repo;
    private readonly IPricingRuleRepository _pricingRepo;
    private readonly IBookingLock _lock;
    private readonly IAuditService _audit;

    public BookingService(
        IBookingRepository repo,
        IPricingRuleRepository pricingRepo,
        IBookingLock bookingLock,
        IAuditService audit)
    {
        _repo = repo;
        _pricingRepo = pricingRepo;
        _lock = bookingLock;
        _audit = audit;
    }

    public async Task<PagedResult<BookingDto>> GetPagedAsync(BookingQuery query, CurrentUser user)
    {
        // Customer luôn bị ép lọc theo chính mình, bỏ qua customerId client gửi lên
        if (!user.IsAdmin)
        {
            if (user.CustomerId == null)
                throw new UnauthorizedAccessException(NoPermission);

            query.CustomerId = user.CustomerId;
        }

        var sortBy = (query.SortBy ?? "checkin").ToLower();
        if (!AllowedSortColumns.Contains(sortBy))
            throw new ArgumentException("sortBy chỉ nhận: checkIn, checkOut, price, status");

        // Không truyền sortBy thì mặc định ngày nhận phòng mới nhất trước (giữ hành vi cũ)
        var desc = query.SortBy == null
            || query.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

        var (items, totalItems) = await _repo.GetPagedAsync(query, sortBy, desc);

        return new PagedResult<BookingDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<BookingDto> GetByIdAsync(int id, CurrentUser user)
    {
        var booking = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {id}");

        EnsureCanAccess(booking.CustomerId, user);
        return ToDto(booking);
    }

    public async Task<List<RoomDto>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId)
    {
        if (checkOut.Date <= checkIn.Date)
            throw new ArgumentException("Ngày trả phòng phải sau ngày nhận phòng");

        if ((checkOut.Date - checkIn.Date).Days > PriceCalculator.MaxNights)
            throw new ArgumentException($"Chỉ báo giá tối đa {PriceCalculator.MaxNights} đêm");

        var from = checkIn.Date;
        var to = checkOut.Date;
        var rooms = await _repo.GetAvailableRoomsAsync(from, to, roomTypeId);
        var quotesByRoomType = new Dictionary<int, PriceBreakdown>();

        foreach (var room in rooms)
        {
            if (!quotesByRoomType.ContainsKey(room.RoomTypeId))
            {
                var rules = await _pricingRepo.GetApplicableAsync(room.RoomTypeId, from, to.AddDays(-1));
                var quote = PriceCalculator.Calculate(from, to, room.RoomType!.PricePerNight, rules);
                quotesByRoomType.Add(room.RoomTypeId, quote);
            }
        }

        return rooms.Select(r =>
        {
            var quote = quotesByRoomType[r.RoomTypeId];
            var roomDto = ToRoomDto(r);
            roomDto.CheckInDate = from;
            roomDto.CheckOutDate = to;
            roomDto.Nights = (to - from).Days;
            roomDto.QuotedRoomCharge = quote.Total;
            roomDto.QuoteItems = quote.Nights.Select(n => new QuoteNightDto
            {
                Date = n.Date,
                DayOfWeek = n.Date.DayOfWeek.ToString(),
                BasePrice = n.BasePrice,
                Price = n.Price,
                RuleName = n.RuleName,
                Percent = n.Percent
            }).ToList();
            return roomDto;
        }).ToList();
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto, CurrentUser user)
    {
        // Admin/Staff đặt giúp khách (dùng dto.CustomerId), Customer chỉ đặt cho chính mình (lấy từ token)
        var customerId = user.IsAdmin ? dto.CustomerId : user.CustomerId;
        if (customerId == null)
            throw new UnauthorizedAccessException(NoPermission);

        var checkIn = dto.CheckInDate.Date;
        var checkOut = dto.CheckOutDate.Date;

        // 1. Kiểm tra ngày
        ValidateDates(checkIn, checkOut);

        // 2. Kiểm tra khách hàng
        if (!await _repo.CustomerExistsAsync(customerId.Value))
            throw new ArgumentException("Khách hàng không tồn tại");

        // 3. Kiểm tra phòng
        var room = await _repo.GetRoomWithTypeAsync(dto.RoomId)
            ?? throw new ArgumentException("Phòng không tồn tại");

        if (room.Status != "Available")
            throw new ArgumentException("Phòng đang bảo trì, không thể đặt");

        // Từ đây đến lúc commit, chỉ một request được xử lý cho phòng này.
        // Thoát hàm mà chưa Commit (do lỗi) thì transaction tự rollback khi Dispose
        await using var tx = await _lock.BeginRoomLockAsync(dto.RoomId);

        // 4. Chống trùng lịch
        if (await _repo.HasOverlapAsync(dto.RoomId, checkIn, checkOut))
            throw new ArgumentException("Phòng đã có người đặt trong khoảng thời gian này");

        // 5. Kiểm tra dịch vụ đi kèm
        var resolved = await ResolveServicesAsync(dto.Services);

        var booking = new Booking
        {
            CustomerId = customerId.Value,
            RoomId = dto.RoomId,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            Status = BookingStatuses.Pending
        };

        foreach (var (service, quantity) in resolved)
        {
            EnsureActive(service);

            // Chụp giá hiện tại của dịch vụ vào UnitPrice
            booking.ExtraServices.Add(new BookingExtraService
            {
                ExtraServiceId = service.Id,
                Quantity = quantity,
                UnitPrice = service.Price
            });
        }

        // 6. Tổng tạm tính (luôn tính ở server): tiền phòng theo từng đêm + tiền dịch vụ
        var roomCharge = await CalculateRoomChargeAsync(
            room.RoomTypeId, room.RoomType!.PricePerNight, checkIn, checkOut);

        var subtotal = roomCharge + ServicesTotal(booking.ExtraServices);

        // 7. Áp mã giảm giá (nếu có)
        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            var code = dto.CouponCode.Trim().ToUpperInvariant();

            var coupon = await _repo.GetCouponByCodeAsync(code)
                ?? throw new ArgumentException("Mã giảm giá không tồn tại");

            var discount = CouponRules.Evaluate(coupon, subtotal, DateTime.Today);

            if (await _repo.CustomerUsedCouponAsync(customerId.Value, coupon.Id))
                throw new ArgumentException("Bạn đã dùng mã giảm giá này rồi");

            // Tăng lượt dùng cùng một SaveChanges với việc lưu booking.
            // RowVersion của Coupon làm hai người cùng lấy lượt cuối không thể cùng thành công
            coupon.UsedCount++;
            booking.CouponId = coupon.Id;
            booking.DiscountAmount = discount;
        }

        booking.TotalPrice = subtotal - booking.DiscountAmount;

        await _repo.AddAsync(booking);
        await _repo.SaveChangesAsync();

        // Lưu lần đầu xong mới có booking.Id; lần lưu thứ hai vẫn nằm trong transaction
        _audit.Log(AuditActions.Create, "Booking", booking.Id,
            $"Phòng {room.RoomNumber}, {checkIn:dd/MM/yyyy} - {checkOut:dd/MM/yyyy}, tổng {booking.TotalPrice:N0} đ, khách Id {customerId}");
        await _repo.SaveChangesAsync();

        // Nhả khóa phòng
        await tx.CommitAsync();

        // Đọc lại để có đủ tên khách, số phòng, tên loại phòng, tên dịch vụ, mã giảm giá
        var saved = await _repo.GetByIdAsync(booking.Id);
        return ToDto(saved!);
    }

    public async Task UpdateDatesAsync(int id, UpdateBookingDatesDto dto, CurrentUser user)
    {
        var roomId = await _repo.GetRoomIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {id}");

        await using var tx = await _lock.BeginRoomAndBookingLockAsync(roomId, id);
        var booking = await _repo.GetForUpdateWithRoomAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {id}");

        EnsureCanAccess(booking.CustomerId, user);

        if (booking.PaymentClaims.Any(x => x.Status == PaymentClaimStatuses.Pending))
            throw new ArgumentException("Không thể đổi ngày khi đang chờ thu ngân xác nhận thanh toán");

        if (booking.Status != BookingStatuses.Pending)
            throw new ArgumentException("Chỉ được đổi ngày khi booking đang ở trạng thái Pending");

        var checkIn = dto.CheckInDate.Date;
        var checkOut = dto.CheckOutDate.Date;

        ValidateDates(checkIn, checkOut);

        // Chống trùng lịch, loại trừ chính booking đang sửa
        if (await _repo.HasOverlapAsync(booking.RoomId, checkIn, checkOut, excludeBookingId: id))
            throw new ArgumentException("Phòng đã có người đặt trong khoảng thời gian này");

        var oldIn = booking.CheckInDate;
        var oldOut = booking.CheckOutDate;

        booking.CheckInDate = checkIn;
        booking.CheckOutDate = checkOut;

        // Tiền phòng tính lại theo từng đêm với quy tắc hiện hành, tiền dịch vụ giữ nguyên
        var roomCharge = await CalculateRoomChargeAsync(
            booking.Room!.RoomTypeId, booking.Room.RoomType!.PricePerNight, checkIn, checkOut);

        ApplyTotals(booking, roomCharge + ServicesTotal(booking.ExtraServices));

        _audit.Log(AuditActions.UpdateDates, "Booking", id,
            $"{oldIn:dd/MM/yyyy}-{oldOut:dd/MM/yyyy} → {checkIn:dd/MM/yyyy}-{checkOut:dd/MM/yyyy}, tổng mới {booking.TotalPrice:N0} đ");

        await _repo.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task UpdateServicesAsync(int id, UpdateBookingServicesDto dto, CurrentUser user)
    {
        await using var tx = await _lock.BeginBookingLockAsync(id);
        var booking = await _repo.GetForUpdateWithRoomAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {id}");

        EnsureCanAccess(booking.CustomerId, user);

        if (booking.PaymentClaims.Any(x => x.Status == PaymentClaimStatuses.Pending))
            throw new ArgumentException("Không thể đổi dịch vụ khi đang chờ thu ngân xác nhận thanh toán");

        // Cho đổi dịch vụ cả khi khách đang ở (gọi thêm trong lúc lưu trú)
        if (booking.Status is not (BookingStatuses.Pending or BookingStatuses.Confirmed or BookingStatuses.CheckedIn))
            throw new ArgumentException("Chỉ đổi được dịch vụ khi booking đang Pending, Confirmed hoặc CheckedIn");

        var resolved = await ResolveServicesAsync(dto.Services);

        var oldTotal = booking.TotalPrice;

        // Tách tiền phòng ra trước khi sửa dịch vụ: tạm tính cũ = TotalPrice + DiscountAmount.
        // Tiền phòng đã chốt nên không bị tính lại theo quy tắc giá mới
        var roomCharge = booking.TotalPrice + booking.DiscountAmount - ServicesTotal(booking.ExtraServices);

        // 1. Xóa dòng không còn trong danh sách mới
        var removed = booking.ExtraServices
            .Where(l => !resolved.Any(r => r.Service.Id == l.ExtraServiceId))
            .ToList();

        foreach (var line in removed)
            booking.ExtraServices.Remove(line);

        // 2. Dòng có sẵn thì sửa số lượng (giữ UnitPrice cũ), dòng mới thì thêm với giá hiện tại
        foreach (var (service, quantity) in resolved)
        {
            var existing = booking.ExtraServices.FirstOrDefault(l => l.ExtraServiceId == service.Id);

            if (existing != null)
            {
                existing.Quantity = quantity;
            }
            else
            {
                EnsureActive(service);

                booking.ExtraServices.Add(new BookingExtraService
                {
                    ExtraServiceId = service.Id,
                    Quantity = quantity,
                    UnitPrice = service.Price
                });
            }
        }

        ApplyTotals(booking, roomCharge + ServicesTotal(booking.ExtraServices));

        _audit.Log(AuditActions.UpdateServices, "Booking", id,
            $"{resolved.Count} dịch vụ, tổng tiền {oldTotal:N0} đ → {booking.TotalPrice:N0} đ");

        await _repo.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task ConfirmAsync(int id)
    {
        var booking = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {id}");

        BookingStateMachine.EnsureTransition(booking.Status, BookingStatuses.Confirmed);

        var from = booking.Status;
        booking.Status = BookingStatuses.Confirmed;

        _audit.Log(AuditActions.Confirm, "Booking", id, $"{from} → {booking.Status}");

        await _repo.SaveChangesAsync();
    }

    public async Task CheckInAsync(int id)
    {
        // Nạp kèm sổ giao dịch để kiểm tra tiền cọc
        var booking = await _repo.GetForUpdateWithPaymentsAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {id}");

        // 1. Đúng trạng thái xuất phát
        BookingStateMachine.EnsureTransition(booking.Status, BookingStatuses.CheckedIn);

        // 2. Đúng thời gian: từ ngày nhận phòng đến trước ngày trả phòng
        var today = DateTime.Today;

        if (today < booking.CheckInDate.Date)
            throw new ArgumentException($"Chưa đến ngày nhận phòng ({booking.CheckInDate:dd/MM/yyyy})");

        if (today >= booking.CheckOutDate.Date)
            throw new ArgumentException("Đã quá ngày trả phòng, không thể nhận phòng");

        // 3. Đủ tiền cọc
        var netPaid = PaymentRules.NetPaid(booking.Payments);
        var deposit = PaymentRules.RequiredDeposit(booking.TotalPrice);

        if (netPaid < deposit)
            throw new ArgumentException(
                $"Cần thanh toán tối thiểu {deposit:N0} đ ({PaymentRules.DepositPercent:0}%) trước khi nhận phòng, hiện đã thu {netPaid:N0} đ");

        booking.Status = BookingStatuses.CheckedIn;
        booking.CheckedInAt = DateTime.UtcNow;

        _audit.Log(AuditActions.CheckIn, "Booking", id, $"Đã thu {netPaid:N0} đ / cọc tối thiểu {deposit:N0} đ");

        await _repo.SaveChangesAsync();
    }

    public async Task CheckOutAsync(int id)
    {
        var booking = await _repo.GetForUpdateWithPaymentsAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {id}");

        BookingStateMachine.EnsureTransition(booking.Status, BookingStatuses.CheckedOut);

        // Phải thanh toán nốt (kể cả dịch vụ gọi thêm trong lúc ở) mới được trả phòng
        var netPaid = PaymentRules.NetPaid(booking.Payments);
        var balance = PaymentRules.BalanceDue(booking.TotalPrice, netPaid, booking.Status);

        if (balance > 0)
            throw new ArgumentException($"Còn nợ {balance:N0} đ, vui lòng thanh toán trước khi trả phòng");

        // Trả phòng sớm hơn dự kiến vẫn được, ta chỉ ghi lại thời điểm thực tế
        booking.Status = BookingStatuses.CheckedOut;
        booking.CheckedOutAt = DateTime.UtcNow;

        _audit.Log(AuditActions.CheckOut, "Booking", id, $"Tổng {booking.TotalPrice:N0} đ, đã thu {netPaid:N0} đ");

        await _repo.SaveChangesAsync();
    }

    public async Task CancelAsync(int id, CurrentUser user)
    {
        await using var tx = await _lock.BeginBookingLockAsync(id);
        // Nạp kèm sổ giao dịch để biết khách đã trả bao nhiêu
        var booking = await _repo.GetForUpdateWithPaymentsAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {id}");

        EnsureCanAccess(booking.CustomerId, user);

        BookingStateMachine.EnsureTransition(booking.Status, BookingStatuses.Cancelled);

        if (booking.CheckInDate.Date <= DateTime.Today && booking.Status == BookingStatuses.Confirmed)
            throw new ArgumentException("Không thể hủy booking đã đến ngày nhận phòng");

        var from = booking.Status;
        booking.Status = BookingStatuses.Cancelled;
        var now = DateTime.UtcNow;

        foreach (var claim in booking.PaymentClaims.Where(x => x.Status == PaymentClaimStatuses.Pending))
        {
            claim.Status = PaymentClaimStatuses.Cancelled;
            claim.CashierReviewedAt = now;
            claim.CashierNote = "Booking đã bị hủy trước khi thu ngân xác nhận thanh toán";
        }

        // Hoàn lại một lượt dùng của mã giảm giá
        if (booking.CouponId.HasValue)
        {
            var coupon = await _repo.GetCouponForUpdateAsync(booking.CouponId.Value);
            if (coupon != null && coupon.UsedCount > 0)
                coupon.UsedCount--;
        }

        // Hoàn tiền tự động theo chính sách: ghi thêm một dòng Refund vào sổ
        var netPaid = PaymentRules.NetPaid(booking.Payments);
        var daysBeforeCheckIn = (booking.CheckInDate.Date - DateTime.Today).Days;
        var refund = RefundPolicy.Calculate(netPaid, daysBeforeCheckIn);

        if (refund.Amount > 0)
        {
            booking.Payments.Add(new Payment
            {
                Type = PaymentRules.TypeRefund,
                Amount = refund.Amount,
                Method = "Original",
                Note = $"Hoàn {refund.Percent:0.##}% theo chính sách hủy (hủy trước ngày nhận phòng {daysBeforeCheckIn} ngày)",
                CreatedAt = DateTime.UtcNow
            });
        }

        _audit.Log(AuditActions.Cancel, "Booking", id,
            $"{from} → Cancelled, đã thu {netPaid:N0} đ, hoàn {refund.Amount:N0} đ ({refund.Percent:0.##}%)");

        // Đổi trạng thái, hoàn lượt mã, ghi hoàn tiền và ghi log cùng thành công hoặc cùng thất bại.
        // Nếu booking vừa bị người khác đổi, RowVersion không khớp và ném DbUpdateConcurrencyException (409)
        await _repo.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var booking = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy booking có Id = {id}");

        if (booking.Status != BookingStatuses.Cancelled)
            throw new ArgumentException("Chỉ được xóa booking đã hủy");

        // Giữ lịch sử tài chính: đã có giao dịch thì không xóa
        if (await _repo.BookingHasPaymentHistoryAsync(id))
            throw new ArgumentException("Không thể xóa booking đã có lịch sử thanh toán");

        // Ghi log trước khi xóa: dòng log không có khóa ngoại nên vẫn còn sau khi booking biến mất
        _audit.Log(AuditActions.Delete, "Booking", id,
            $"Xóa booking đã hủy: phòng {booking.RoomId}, khách {booking.CustomerId}, {booking.CheckInDate:dd/MM/yyyy} - {booking.CheckOutDate:dd/MM/yyyy}");

        // Các dòng dịch vụ của booking tự bị xóa theo (cascade ở database)
        _repo.Remove(booking);
        await _repo.SaveChangesAsync();
    }

    // ===== Hàm hỗ trợ =====
    private const string NoPermission = "Bạn không có quyền thực hiện thao tác này";

    // Admin/Staff được truy cập mọi booking, Customer chỉ booking của chính mình
    private static void EnsureCanAccess(int bookingCustomerId, CurrentUser user)
    {
        if (!user.IsAdmin && user.CustomerId != bookingCustomerId)
            throw new UnauthorizedAccessException(NoPermission);
    }

    private static void ValidateDates(DateTime checkIn, DateTime checkOut)
    {
        if (checkIn < DateTime.Today)
            throw new ArgumentException("Ngày nhận phòng không được nằm trong quá khứ");

        if (checkOut <= checkIn)
            throw new ArgumentException("Ngày trả phòng phải sau ngày nhận phòng");

        if ((checkOut - checkIn).Days > PriceCalculator.MaxNights)
            throw new ArgumentException($"Chỉ được đặt tối đa {PriceCalculator.MaxNights} đêm");
    }

    // Lấy các quy tắc giá liên quan rồi để PriceCalculator tính từng đêm
    private async Task<decimal> CalculateRoomChargeAsync(
        int roomTypeId, decimal basePrice, DateTime checkIn, DateTime checkOut)
    {
        var rules = await _pricingRepo.GetApplicableAsync(roomTypeId, checkIn, checkOut.AddDays(-1));
        return PriceCalculator.Calculate(checkIn, checkOut, basePrice, rules).Total;
    }

    private static decimal ServicesTotal(IEnumerable<BookingExtraService> lines) =>
        lines.Sum(x => x.Quantity * x.UnitPrice);

    // Giữ bất biến: TotalPrice = tạm tính - DiscountAmount, và DiscountAmount không vượt tạm tính
    private static void ApplyTotals(Booking booking, decimal subtotal)
    {
        booking.DiscountAmount = Math.Min(booking.DiscountAmount, subtotal);
        booking.TotalPrice = subtotal - booking.DiscountAmount;
    }

    // Kiểm tra danh sách dịch vụ client gửi: không trùng, tồn tại. Trả về (dịch vụ, số lượng)
    private async Task<List<(ExtraService Service, int Quantity)>> ResolveServicesAsync(
        List<BookingServiceItemDto>? items)
    {
        if (items == null || items.Count == 0)
            return new List<(ExtraService, int)>();

        var ids = items.Select(x => x.ExtraServiceId).ToList();

        if (ids.Distinct().Count() != ids.Count)
            throw new ArgumentException("Dịch vụ bị trùng, hãy gộp thành một dòng và tăng số lượng");

        var found = await _repo.GetExtraServicesByIdsAsync(ids);

        if (found.Count != ids.Count)
            throw new ArgumentException("Có dịch vụ không tồn tại");

        return items
            .Select(i => (found.First(f => f.Id == i.ExtraServiceId), i.Quantity))
            .ToList();
    }

    private static void EnsureActive(ExtraService service)
    {
        if (!service.IsActive)
            throw new ArgumentException($"Dịch vụ '{service.Name}' hiện không còn cung cấp");
    }

    private static BookingDto ToDto(Booking x)
    {
        var paid = PaymentRules.TotalPaid(x.Payments);
        var refunded = PaymentRules.TotalRefunded(x.Payments);

        return new BookingDto
        {
            Id = x.Id,
            CustomerId = x.CustomerId,
            CustomerName = x.Customer?.FullName ?? string.Empty,
            RoomId = x.RoomId,
            RoomNumber = x.Room?.RoomNumber ?? string.Empty,
            RoomTypeName = x.Room?.RoomType?.Name ?? string.Empty,
            CheckInDate = x.CheckInDate,
            CheckOutDate = x.CheckOutDate,
            Services = x.ExtraServices
                .OrderBy(s => s.ExtraServiceId)
                .Select(s => new BookingServiceDto
                {
                    ExtraServiceId = s.ExtraServiceId,
                    Name = s.ExtraService?.Name ?? string.Empty,
                    Unit = s.ExtraService?.Unit ?? string.Empty,
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice
                })
                .ToList(),
            ServicesTotal = ServicesTotal(x.ExtraServices),
            CouponCode = x.Coupon?.Code,
            DiscountAmount = x.DiscountAmount,
            TotalPrice = x.TotalPrice,
            TotalPaid = paid,
            TotalRefunded = refunded,
            BalanceDue = PaymentRules.BalanceDue(x.TotalPrice, paid - refunded, x.Status),
            PaymentStatus = PaymentRules.GetStatus(x.TotalPrice, paid, refunded, x.Status),
            Status = x.Status,
            CheckedInAt = x.CheckedInAt,
            CheckedOutAt = x.CheckedOutAt
        };
    }

    private static RoomDto ToRoomDto(Room x) => new()
    {
        Id = x.Id,
        RoomNumber = x.RoomNumber,
        Status = x.Status,
        RoomTypeId = x.RoomTypeId,
        RoomTypeName = x.RoomType?.Name ?? string.Empty,
        PricePerNight = x.RoomType?.PricePerNight ?? 0,
        Floor = x.Floor,
        Description = x.Description,
        ImageUrl = x.ImageUrl,
        Amenities = x.Amenities.Select(a => a.Name).ToList()
    };
}