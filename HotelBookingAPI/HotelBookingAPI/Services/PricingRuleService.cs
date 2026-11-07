using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class PricingRuleService : IPricingRuleService
{
    private readonly IPricingRuleRepository _repo;

    public PricingRuleService(IPricingRuleRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<PricingRuleDto>> GetAllAsync(bool activeOnly)
    {
        var items = await _repo.GetAllAsync(activeOnly);
        return items.Select(ToDto).ToList();
    }

    public async Task<PricingRuleDto> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy quy tắc giá có Id = {id}");

        return ToDto(item);
    }

    public async Task<PricingRuleDto> CreateAsync(CreatePricingRuleDto dto)
    {
        var name = dto.Name.Trim();

        if (await _repo.NameExistsAsync(name))
            throw new ArgumentException("Tên quy tắc đã tồn tại");

        await ValidateInputAsync(dto);

        var entity = new PricingRule();
        Apply(entity, dto, name);

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        // Đọc lại để có tên loại phòng
        var saved = await _repo.GetByIdAsync(entity.Id);
        return ToDto(saved!);
    }

    public async Task UpdateAsync(int id, UpdatePricingRuleDto dto)
    {
        var entity = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy quy tắc giá có Id = {id}");

        var name = dto.Name.Trim();

        if (await _repo.NameExistsAsync(name, excludeId: id))
            throw new ArgumentException("Tên quy tắc đã tồn tại");

        await ValidateInputAsync(dto);

        // Đổi quy tắc chỉ ảnh hưởng booking tạo hoặc đổi ngày SAU này
        Apply(entity, dto, name);
        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy quy tắc giá có Id = {id}");

        // Booking chỉ lưu tổng tiền đã chốt, không tham chiếu quy tắc, nên xóa thoải mái
        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
    }

    public async Task<QuoteDto> GetQuoteAsync(int roomId, DateTime checkIn, DateTime checkOut)
    {
        var from = checkIn.Date;
        var to = checkOut.Date;

        if (to <= from)
            throw new ArgumentException("Ngày trả phòng phải sau ngày nhận phòng");

        if ((to - from).Days > PriceCalculator.MaxNights)
            throw new ArgumentException($"Chỉ báo giá tối đa {PriceCalculator.MaxNights} đêm");

        var room = await _repo.GetRoomWithTypeAsync(roomId)
            ?? throw new KeyNotFoundException($"Không tìm thấy phòng có Id = {roomId}");

        var basePrice = room.RoomType!.PricePerNight;
        var rules = await _repo.GetApplicableAsync(room.RoomTypeId, from, to.AddDays(-1));
        var result = PriceCalculator.Calculate(from, to, basePrice, rules);

        return new QuoteDto
        {
            RoomId = room.Id,
            RoomNumber = room.RoomNumber,
            RoomTypeName = room.RoomType.Name,
            Nights = result.Nights.Count,
            BasePricePerNight = basePrice,
            RoomCharge = result.Total,
            Items = result.Nights.Select(n => new QuoteNightDto
            {
                Date = n.Date,
                DayOfWeek = n.Date.DayOfWeek.ToString(),
                BasePrice = n.BasePrice,
                Price = n.Price,
                RuleName = n.RuleName,
                Percent = n.Percent
            }).ToList()
        };
    }

    // Luật liên quan nhiều trường với nhau nên kiểm tra ở Service
    private async Task ValidateInputAsync(PricingRuleInputDto dto)
    {
        if (dto.RuleType == "Season")
        {
            if (dto.StartDate == null || dto.EndDate == null)
                throw new ArgumentException("Quy tắc theo mùa phải có ngày bắt đầu và ngày kết thúc");

            if (dto.EndDate.Value.Date < dto.StartDate.Value.Date)
                throw new ArgumentException("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu");
        }
        else if (dto.StartDate != null || dto.EndDate != null)
        {
            throw new ArgumentException("Quy tắc Weekend không dùng ngày bắt đầu và ngày kết thúc");
        }

        if (dto.Percent == 0)
            throw new ArgumentException("Phần trăm điều chỉnh phải khác 0");

        if (dto.RoomTypeId.HasValue && !await _repo.RoomTypeExistsAsync(dto.RoomTypeId.Value))
            throw new ArgumentException("Loại phòng không tồn tại");
    }

    private static void Apply(PricingRule entity, PricingRuleInputDto dto, string name)
    {
        entity.Name = name;
        entity.RuleType = dto.RuleType;
        entity.StartDate = dto.StartDate?.Date;
        entity.EndDate = dto.EndDate?.Date;
        entity.RoomTypeId = dto.RoomTypeId;
        entity.Percent = dto.Percent;
        entity.Priority = dto.Priority;
        entity.IsActive = dto.IsActive;
    }

    private static PricingRuleDto ToDto(PricingRule x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        RuleType = x.RuleType,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        RoomTypeId = x.RoomTypeId,
        RoomTypeName = x.RoomType?.Name,
        Percent = x.Percent,
        Priority = x.Priority,
        IsActive = x.IsActive
    };
}