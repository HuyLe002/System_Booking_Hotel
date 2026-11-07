using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);     // chỉ đọc, dùng khi đăng nhập
    Task<User?> GetForUpdateAsync(int id);               // có tracking
    Task<List<User>> GetAllAsync();
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> CustomerEmailExistsAsync(string email);
    Task AddAsync(User user);
    void Remove(User user);
    Task SaveChangesAsync();
}