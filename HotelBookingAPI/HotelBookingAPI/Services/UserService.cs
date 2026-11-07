using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _repo.GetAllAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<UserDto> CreateStaffAsync(CreateStaffDto dto)
    {
        var username = dto.Username.Trim();

        if (await _repo.UsernameExistsAsync(username))
            throw new ArgumentException("Tên đăng nhập đã tồn tại");

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Roles.Staff
        };

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task DeleteStaffAsync(int id)
    {
        var user = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy tài khoản có Id = {id}");

        // Chặn xóa nhầm Admin hoặc tài khoản của khách hàng
        if (user.Role != Roles.Staff)
            throw new ArgumentException("Chỉ được xóa tài khoản Staff");

        // Refresh token của tài khoản này tự bị xóa theo (cascade)
        _repo.Remove(user);
        await _repo.SaveChangesAsync();
    }

    private static UserDto ToDto(User x) => new()
    {
        Id = x.Id,
        Username = x.Username,
        Role = x.Role,
        CustomerId = x.CustomerId
    };
}