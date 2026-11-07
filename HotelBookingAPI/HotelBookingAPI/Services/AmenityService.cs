using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class AmenityService : IAmenityService
{
    private readonly IAmenityRepository _repo;

    public AmenityService(IAmenityRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<AmenityDto>> GetAllAsync(string? keyword)
    {
        var items = await _repo.GetAllAsync(keyword);
        return items.Select(ToDto).ToList();
    }

    public async Task<AmenityDto> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdWithCountAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy tiện nghi có Id = {id}");

        return ToDto(item);
    }

    public async Task<AmenityDto> CreateAsync(CreateAmenityDto dto)
    {
        var name = dto.Name.Trim();

        if (await _repo.NameExistsAsync(name))
            throw new ArgumentException("Tên tiện nghi đã tồn tại");

        var amenity = new Amenity { Name = name };

        await _repo.AddAsync(amenity);
        await _repo.SaveChangesAsync();

        // Tiện nghi vừa tạo chưa gắn với phòng nào
        return ToDto(new AmenityWithRoomCount(amenity, 0));
    }

    public async Task UpdateAsync(int id, UpdateAmenityDto dto)
    {
        var amenity = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy tiện nghi có Id = {id}");

        var name = dto.Name.Trim();

        if (await _repo.NameExistsAsync(name, excludeId: id))
            throw new ArgumentException("Tên tiện nghi đã tồn tại");

        amenity.Name = name;
        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var amenity = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy tiện nghi có Id = {id}");

        if (await _repo.IsUsedByRoomsAsync(id))
            throw new ArgumentException("Không thể xóa vì tiện nghi đang được gán cho phòng");

        _repo.Remove(amenity);
        await _repo.SaveChangesAsync();
    }

    private static AmenityDto ToDto(AmenityWithRoomCount x) => new()
    {
        Id = x.Amenity.Id,
        Name = x.Amenity.Name,
        TotalRooms = x.TotalRooms
    };
}