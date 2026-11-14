using System.ComponentModel.DataAnnotations;
using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;
using HotelBookingAPI.Services;

namespace HotelBookingAPI.Tests;

public class AuthRegistrationTests
{
    [Theory]
    [InlineData("0900000001", true)]
    [InlineData("+84900000001", true)]
    [InlineData("090000000", false)]
    [InlineData("09000000012", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void RegisterDtoValidatesPhone(string? phone, bool expectedValid)
    {
        var dto = new RegisterDto();
        var context = new ValidationContext(dto) { MemberName = nameof(RegisterDto.Phone) };
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateProperty(phone, context, results);

        Assert.Equal(expectedValid, valid);
    }

    [Fact]
    public async Task RegisterCreatesCustomerAccountAndTrimsTextFields()
    {
        var users = new TestUserRepository();
        var service = new AuthService(users, new TestRefreshTokenRepository(), new TestJwtTokenGenerator());

        var customerId = await service.RegisterAsync(new RegisterDto
        {
            Username = "  guest01  ",
            Password = "secret123",
            FullName = "  Guest User  ",
            Email = "  guest@example.com  ",
            Phone = " 0900000001 "
        });

        Assert.Equal(73, customerId);
        var createdUser = Assert.IsType<User>(users.CreatedUser);
        Assert.Equal("guest01", createdUser.Username);
        Assert.Equal(Roles.Customer, createdUser.Role);
        Assert.Equal("Guest User", createdUser.Customer!.FullName);
        Assert.Equal("guest@example.com", createdUser.Customer.Email);
        Assert.Equal("0900000001", createdUser.Customer.Phone);
    }

    [Theory]
    [InlineData("   ", "0900000001")]
    [InlineData("guest01", "invalid")]
    public async Task RegisterRejectsWhitespaceUsernameAndInvalidPhone(string username, string phone)
    {
        var users = new TestUserRepository();
        var service = new AuthService(users, new TestRefreshTokenRepository(), new TestJwtTokenGenerator());

        await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterAsync(new RegisterDto
        {
            Username = username,
            Password = "secret123",
            FullName = "Guest User",
            Email = "guest@example.com",
            Phone = phone
        }));

        Assert.Null(users.CreatedUser);
    }

    private sealed class TestUserRepository : IUserRepository
    {
        public User? CreatedUser { get; private set; }

        public Task<User?> GetByUsernameAsync(string username) => Task.FromResult<User?>(null);

        public Task<User?> GetForUpdateAsync(int id) => Task.FromResult<User?>(null);

        public Task<List<User>> GetAllAsync() => Task.FromResult(new List<User>());

        public Task<bool> UsernameExistsAsync(string username) => Task.FromResult(false);

        public Task<bool> CustomerEmailExistsAsync(string email) => Task.FromResult(false);

        public Task AddAsync(User user)
        {
            CreatedUser = user;
            return Task.CompletedTask;
        }

        public void Remove(User user) { }

        public Task SaveChangesAsync()
        {
            CreatedUser!.Customer!.Id = 73;
            CreatedUser!.CustomerId = 73;
            return Task.CompletedTask;
        }
    }

    private sealed class TestRefreshTokenRepository : IRefreshTokenRepository
    {
        public Task<RefreshToken?> GetByHashAsync(string tokenHash) =>
            Task.FromResult<RefreshToken?>(null);

        public Task AddAsync(RefreshToken token) => Task.CompletedTask;

        public Task RevokeAllForUserAsync(int userId) => Task.CompletedTask;

        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private sealed class TestJwtTokenGenerator : IJwtTokenGenerator
    {
        public (string Token, DateTime Expires) Generate(User user) =>
            ("access-token", DateTime.UtcNow);

        public (string Raw, string Hash, DateTime Expires) GenerateRefreshToken() =>
            ("refresh-token", "refresh-hash", DateTime.UtcNow);

        public string HashToken(string rawToken) => "refresh-hash";
    }
}
