using HotelBookingAPI.Models;

namespace HotelBookingAPI.Services;

public interface IJwtTokenGenerator
{
    (string Token, DateTime Expires) Generate(User user);

    // Raw: gửi cho client. Hash: lưu database. Expires: hạn của refresh token
    (string Raw, string Hash, DateTime Expires) GenerateRefreshToken();

    string HashToken(string rawToken);
}