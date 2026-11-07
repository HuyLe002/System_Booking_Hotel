using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class RoomTypeService : IRoomTypeService
{
    private readonly IRoomTypeRepository _repo;

    public RoomTypeService(IRoomTypeRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<RoomTypeDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.Select(ToDto).ToList();
    }

    public async Task<RoomTypeDto> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy loại phòng có Id = {id}");

        return ToDto(entity);
    }

    public async Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto dto)
    {
        if (await _repo.NameExistsAsync(dto.Name))
            throw new ArgumentException("Tên loại phòng đã tồn tại");

        var entity = new RoomType
        {
            Name = dto.Name,
            Description = dto.Description,
            PricePerNight = dto.PricePerNight,
            Capacity = dto.Capacity
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        return ToDto(entity);
    }

    public async Task UpdateAsync(int id, UpdateRoomTypeDto dto)
    {
        var entity = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy loại phòng có Id = {id}");

        if (await _repo.NameExistsAsync(dto.Name, excludeId: id))
            throw new ArgumentException("Tên loại phòng đã tồn tại");

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.PricePerNight = dto.PricePerNight;
        entity.Capacity = dto.Capacity;

        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy loại phòng có Id = {id}");

        if (await _repo.HasRoomsAsync(id))
            throw new ArgumentException("Không thể xóa vì loại phòng đang có phòng sử dụng");

        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
    }

    // Đổi Model sang DTO ở một chỗ duy nhất
    private static RoomTypeDto ToDto(RoomType x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        PricePerNight = x.PricePerNight,
        Capacity = x.Capacity
    };
}