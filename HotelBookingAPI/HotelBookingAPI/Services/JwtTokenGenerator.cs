using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HotelBookingAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace HotelBookingAPI.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime Expires) Generate(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.CustomerId.HasValue)
            claims.Add(new Claim("customerId", user.CustomerId.Value.ToString()));

        var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    // Chuỗi ngẫu nhiên 64 byte từ bộ sinh số an toàn (không dùng Random thường)
    public (string Raw, string Hash, DateTime Expires) GenerateRefreshToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var days = _config.GetValue("Jwt:RefreshExpireDays", 7);

        return (raw, HashToken(raw), DateTime.UtcNow.AddDays(days));
    }

    public string HashToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}