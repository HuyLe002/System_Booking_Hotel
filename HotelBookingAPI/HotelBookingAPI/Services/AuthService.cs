using System.Text.RegularExpressions;
﻿using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _tokens;
    private readonly IJwtTokenGenerator _jwt;

    public AuthService(IUserRepository users, IRefreshTokenRepository tokens, IJwtTokenGenerator jwt)
    {
        _users = users;
        _tokens = tokens;
        _jwt = jwt;
    }

    // Tạo Customer và User trong cùng một lần lưu
    public async Task<int> RegisterAsync(RegisterDto dto)
    {
        var username = dto.Username.Trim();
        var fullName = dto.FullName.Trim();
        var email = dto.Email.Trim();
        var phone = dto.Phone?.Trim();

        if (username.Length is < 3 or > 50)
            throw new ArgumentException("Tên đăng nhập từ 3 đến 50 ký tự");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Họ tên là bắt buộc");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email là bắt buộc");

        if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, RegisterDto.PhonePattern))
            throw new ArgumentException("Số điện thoại không hợp lệ (VD: 0900000001 hoặc +84900000001)");

        if (await _users.UsernameExistsAsync(username))
            throw new ArgumentException("Tên đăng nhập đã tồn tại");

        if (await _users.CustomerEmailExistsAsync(email))
            throw new ArgumentException("Email đã được sử dụng");

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Roles.Customer,
            Customer = new Customer
            {
                FullName = fullName,
                Email = email,
                Phone = phone
            }
        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        return user.CustomerId!.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByUsernameAsync(dto.Username.Trim());

        // Không tiết lộ là sai tên đăng nhập hay sai mật khẩu
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new InvalidCredentialsException("Sai tên đăng nhập hoặc mật khẩu");

        var response = await IssueTokensAsync(user);
        await _tokens.SaveChangesAsync();
        return response;
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshTokenDto dto)
    {
        var stored = await _tokens.GetByHashAsync(_jwt.HashToken(dto.RefreshToken))
            ?? throw new InvalidCredentialsException("Refresh token không hợp lệ");

        // Token đã bị thu hồi mà vẫn có người đem ra dùng: nghi bị đánh cắp,
        // thu hồi toàn bộ phiên của tài khoản này, buộc đăng nhập lại
        if (stored.RevokedAt != null)
        {
            await _tokens.RevokeAllForUserAsync(stored.UserId);
            throw new InvalidCredentialsException("Refresh token đã bị thu hồi, vui lòng đăng nhập lại");
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidCredentialsException("Refresh token đã hết hạn, vui lòng đăng nhập lại");

        // Xoay vòng: thu hồi token cũ, cấp cặp token mới (cùng một lần lưu)
        stored.RevokedAt = DateTime.UtcNow;

        var response = await IssueTokensAsync(stored.User!);
        await _tokens.SaveChangesAsync();
        return response;
    }

    // Idempotent: token không có hoặc đã thu hồi thì coi như đã đăng xuất, không báo lỗi
    public async Task LogoutAsync(RefreshTokenDto dto)
    {
        var stored = await _tokens.GetByHashAsync(_jwt.HashToken(dto.RefreshToken));

        if (stored == null || stored.RevokedAt != null)
            return;

        stored.RevokedAt = DateTime.UtcNow;
        await _tokens.SaveChangesAsync();
    }

    public async Task LogoutAllAsync(int userId) =>
        await _tokens.RevokeAllForUserAsync(userId);

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _users.GetForUpdateAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài khoản");

        // 400 chứ không phải 401: người dùng đã đăng nhập hợp lệ, chỉ nhập sai dữ liệu
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new ArgumentException("Mật khẩu hiện tại không đúng");

        if (dto.CurrentPassword == dto.NewPassword)
            throw new ArgumentException("Mật khẩu mới phải khác mật khẩu hiện tại");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _users.SaveChangesAsync();

        // Các thiết bị khác phải đăng nhập lại
        await _tokens.RevokeAllForUserAsync(userId);
    }

    // Tạo access token + refresh token mới. Chỉ thêm vào context, người gọi tự SaveChanges
    private async Task<AuthResponseDto> IssueTokensAsync(User user)
    {
        var (token, expires) = _jwt.Generate(user);
        var (raw, hash, refreshExpires) = _jwt.GenerateRefreshToken();

        await _tokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExpires
        });

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expires,
            RefreshToken = raw,
            RefreshTokenExpiresAt = refreshExpires,
            Username = user.Username,
            Role = user.Role
        };
    }
}