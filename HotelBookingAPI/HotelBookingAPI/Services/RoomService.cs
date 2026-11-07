using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class RoomService : IRoomService
{
    private static readonly string[] AllowedSortColumns = { "roomnumber", "price", "status", "floor" };

    private readonly IRoomRepository _repo;

    public RoomService(IRoomRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<RoomDto>> GetPagedAsync(RoomQuery query)
    {
        var sortBy = (query.SortBy ?? "roomnumber").ToLower();
        if (!AllowedSortColumns.Contains(sortBy))
            throw new ArgumentException("sortBy chỉ nhận: roomNumber, price, status, floor");

        var desc = query.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

        var (items, totalItems) = await _repo.GetPagedAsync(query, sortBy, desc);

        return new PagedResult<RoomDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<RoomDto> GetByIdAsync(int id)
    {
        var room = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy phòng có Id = {id}");

        return ToDto(room);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto)
    {
        if (!await _repo.RoomTypeExistsAsync(dto.RoomTypeId))
            throw new ArgumentException("Loại phòng không tồn tại");

        if (await _repo.RoomNumberExistsAsync(dto.RoomNumber))
            throw new ArgumentException("Số phòng đã tồn tại");

        var amenities = await GetValidAmenitiesAsync(dto.AmenityIds);

        var room = new Room
        {
            RoomNumber = dto.RoomNumber,
            Status = dto.Status,
            RoomTypeId = dto.RoomTypeId,
            Floor = dto.Floor,
            Description = NormalizeText(dto.Description),
            ImageUrl = NormalizeText(dto.ImageUrl),
            Amenities = amenities
        };

        await _repo.AddAsync(room);
        await _repo.SaveChangesAsync();

        // Đọc lại để có đủ tên loại phòng, giá và tiện nghi
        var saved = await _repo.GetByIdAsync(room.Id);
        return ToDto(saved!);
    }

    public async Task UpdateAsync(int id, UpdateRoomDto dto)
    {
        var room = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy phòng có Id = {id}");

        if (!await _repo.RoomTypeExistsAsync(dto.RoomTypeId))
            throw new ArgumentException("Loại phòng không tồn tại");

        if (await _repo.RoomNumberExistsAsync(dto.RoomNumber, excludeId: id))
            throw new ArgumentException("Số phòng đã tồn tại");

        var amenities = await GetValidAmenitiesAsync(dto.AmenityIds);

        room.RoomNumber = dto.RoomNumber;
        room.Status = dto.Status;
        room.RoomTypeId = dto.RoomTypeId;
        room.Floor = dto.Floor;
        room.Description = NormalizeText(dto.Description);
        room.ImageUrl = NormalizeText(dto.ImageUrl);

        // Cập nhật quan hệ n-n: xóa hết rồi gán lại
        room.Amenities.Clear();
        foreach (var amenity in amenities)
            room.Amenities.Add(amenity);

        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var room = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy phòng có Id = {id}");

        if (await _repo.HasBookingsAsync(id))
            throw new ArgumentException("Không thể xóa vì phòng đã có lịch đặt");

        room.Amenities.Clear();
        _repo.Remove(room);
        await _repo.SaveChangesAsync();
    }

    // Lấy danh sách tiện nghi, báo lỗi nếu có Id không tồn tại
    private async Task<List<Amenity>> GetValidAmenitiesAsync(List<int> amenityIds)
    {
        var ids = amenityIds.Distinct().ToList();
        var amenities = await _repo.GetAmenitiesByIdsAsync(ids);

        if (amenities.Count != ids.Count)
            throw new ArgumentException("Có tiện nghi không tồn tại");

        return amenities;
    }

    // Cắt khoảng trắng thừa, chuỗi rỗng thì lưu null
    private static string? NormalizeText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static RoomDto ToDto(Room x) => new()
    {
        Id = x.Id,
        RoomNumber = x.RoomNumber,
        Status = x.Status,
        RoomTypeId = x.RoomTypeId,
        RoomTypeName = x.RoomType?.Name ?? string.Empty,
        Capacity = x.RoomType?.Capacity ?? 0,
        PricePerNight = x.RoomType?.PricePerNight ?? 0,
        Floor = x.Floor,
        Description = x.Description,
        ImageUrl = x.ImageUrl,
        Amenities = x.Amenities.Select(a => a.Name).ToList(),
        AmenityIds = x.Amenities.Select(a => a.Id).ToList()
    };
}