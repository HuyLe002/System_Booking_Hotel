using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class CouponService : ICouponService
{
    private readonly ICouponRepository _repo;

    public CouponService(ICouponRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<CouponDto>> GetAllAsync(bool activeOnly)
    {
        var items = await _repo.GetAllAsync(activeOnly);
        return items.Select(ToDto).ToList();
    }

    public async Task<CouponDto> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy mã giảm giá có Id = {id}");

        return ToDto(item);
    }

    public async Task<CouponDto> CreateAsync(CreateCouponDto dto)
    {
        // Mã luôn lưu chữ HOA để so sánh không phân biệt hoa thường
        var code = dto.Code.Trim().ToUpperInvariant();

        if (await _repo.CodeExistsAsync(code))
            throw new ArgumentException("Mã giảm giá đã tồn tại");

        ValidateInput(dto);

        var entity = new Coupon { Code = code };
        Apply(entity, dto);

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        return ToDto(entity);
    }

    public async Task UpdateAsync(int id, UpdateCouponDto dto)
    {
        var entity = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy mã giảm giá có Id = {id}");

        ValidateInput(dto);

        if (dto.MaxUses.HasValue && dto.MaxUses.Value < entity.UsedCount)
            throw new ArgumentException($"Số lượt tối đa không được nhỏ hơn số lượt đã dùng ({entity.UsedCount})");

        // Đổi điều kiện chỉ ảnh hưởng booking tạo SAU này, vì booking cũ đã lưu DiscountAmount riêng
        Apply(entity, dto);
        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy mã giảm giá có Id = {id}");

        if (await _repo.IsUsedAsync(id))
            throw new ArgumentException("Không thể xóa vì mã đã được dùng trong booking. Hãy tắt (isActive = false) thay vì xóa");

        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
    }

    // Luật liên quan nhiều trường với nhau nên kiểm tra ở Service
    private static void ValidateInput(CouponInputDto dto)
    {
        if (dto.DiscountType == "Percent" && dto.Value > 100)
            throw new ArgumentException("Giảm theo phần trăm thì giá trị tối đa là 100");

        if (dto.ValidTo.Date < dto.ValidFrom.Date)
            throw new ArgumentException("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu");
    }

    private static void Apply(Coupon entity, CouponInputDto dto)
    {
        entity.DiscountType = dto.DiscountType;
        entity.Value = dto.Value;
        // Trần giảm tối đa chỉ có nghĩa với kiểu phần trăm
        entity.MaxDiscount = dto.DiscountType == "Percent" ? dto.MaxDiscount : null;
        entity.MinBookingAmount = dto.MinBookingAmount;
        entity.ValidFrom = dto.ValidFrom.Date;
        entity.ValidTo = dto.ValidTo.Date;
        entity.MaxUses = dto.MaxUses;
        entity.IsActive = dto.IsActive;
    }

    private static CouponDto ToDto(Coupon x) => new()
    {
        Id = x.Id,
        Code = x.Code,
        DiscountType = x.DiscountType,
        Value = x.Value,
        MaxDiscount = x.MaxDiscount,
        MinBookingAmount = x.MinBookingAmount,
        ValidFrom = x.ValidFrom,
        ValidTo = x.ValidTo,
        MaxUses = x.MaxUses,
        UsedCount = x.UsedCount,
        IsActive = x.IsActive
    };
}