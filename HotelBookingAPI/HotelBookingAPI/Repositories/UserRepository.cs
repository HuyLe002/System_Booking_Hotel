using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly HotelDbContext _context;

    public UserRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username);

    public async Task<User?> GetForUpdateAsync(int id) =>
        await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<User>> GetAllAsync() =>
        await _context.Users
            .AsNoTracking()
            .OrderBy(x => x.Role)
            .ThenBy(x => x.Username)
            .ToListAsync();

    public async Task<bool> UsernameExistsAsync(string username) =>
        await _context.Users.AnyAsync(x => x.Username == username);

    public async Task<bool> CustomerEmailExistsAsync(string email) =>
        await _context.Customers.AnyAsync(x => x.Email == email);

    public async Task AddAsync(User user) =>
        await _context.Users.AddAsync(user);

    public void Remove(User user) =>
        _context.Users.Remove(user);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}