using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class ExtraServiceService : IExtraServiceService
{
    private readonly IExtraServiceRepository _repo;

    public ExtraServiceService(IExtraServiceRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ExtraServiceDto>> GetAllAsync(bool activeOnly)
    {
        var items = await _repo.GetAllAsync(activeOnly);
        return items.Select(ToDto).ToList();
    }

    public async Task<ExtraServiceDto> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy dịch vụ có Id = {id}");

        return ToDto(item);
    }

    public async Task<ExtraServiceDto> CreateAsync(CreateExtraServiceDto dto)
    {
        var name = dto.Name.Trim();

        if (await _repo.NameExistsAsync(name))
            throw new ArgumentException("Tên dịch vụ đã tồn tại");

        var entity = new ExtraService
        {
            Name = name,
            Price = dto.Price,
            Unit = dto.Unit.Trim(),
            IsActive = dto.IsActive
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        return ToDto(entity);
    }

    public async Task UpdateAsync(int id, UpdateExtraServiceDto dto)
    {
        var entity = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy dịch vụ có Id = {id}");

        var name = dto.Name.Trim();

        if (await _repo.NameExistsAsync(name, excludeId: id))
            throw new ArgumentException("Tên dịch vụ đã tồn tại");

        // Đổi giá chỉ ảnh hưởng booking tạo SAU này, vì booking cũ đã lưu UnitPrice riêng
        entity.Name = name;
        entity.Price = dto.Price;
        entity.Unit = dto.Unit.Trim();
        entity.IsActive = dto.IsActive;

        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy dịch vụ có Id = {id}");

        if (await _repo.IsUsedAsync(id))
            throw new ArgumentException("Không thể xóa vì dịch vụ đã được dùng trong booking. Hãy tắt (isActive = false) thay vì xóa");

        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
    }

    private static ExtraServiceDto ToDto(ExtraService x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Price = x.Price,
        Unit = x.Unit,
        IsActive = x.IsActive
    };
}