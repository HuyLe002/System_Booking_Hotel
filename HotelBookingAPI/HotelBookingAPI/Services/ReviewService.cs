using HotelBookingAPI.Common;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repositories;

namespace HotelBookingAPI.Services;

public class ReviewService : IReviewService
{
    private static readonly string[] AllowedSortColumns = { "date", "rating" };
    private const string NoPermission = "Bạn không có quyền thực hiện thao tác này";

    private readonly IReviewRepository _repo;

    public ReviewService(IReviewRepository repo)
    {
        _repo = repo;
    }

    public async Task<RoomReviewsDto> GetByRoomAsync(int roomId, PagingQuery query)
    {
        var room = await _repo.GetRoomAsync(roomId)
            ?? throw new KeyNotFoundException($"Không tìm thấy phòng có Id = {roomId}");

        var sortBy = (query.SortBy ?? "date").ToLower();
        if (!AllowedSortColumns.Contains(sortBy))
            throw new ArgumentException("sortBy chỉ nhận: date, rating");

        // Không truyền sortBy thì mặc định đánh giá mới nhất trước
        var desc = query.SortBy == null
            || query.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

        var summary = await _repo.GetRatingSummaryAsync(roomId);
        var (items, totalItems) = await _repo.GetPagedByRoomAsync(roomId, query, sortBy, desc);

        return new RoomReviewsDto
        {
            RoomId = room.Id,
            RoomNumber = room.RoomNumber,
            AverageRating = Math.Round(summary.Average, 1),
            TotalReviews = summary.Count,
            Reviews = new PagedResult<ReviewDto>
            {
                Items = items.Select(ToDto).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = totalItems
            }
        };
    }

    public async Task<ReviewDto> GetByIdAsync(int id)
    {
        var review = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy đánh giá có Id = {id}");

        return ToDto(review);
    }

    public async Task<ReviewDto> CreateAsync(CreateReviewDto dto, CurrentUser user)
    {
        // Chỉ tài khoản khách (có customerId) mới được viết đánh giá
        if (user.CustomerId == null)
            throw new UnauthorizedAccessException(NoPermission);

        var booking = await _repo.GetBookingAsync(dto.BookingId)
            ?? throw new ArgumentException("Booking không tồn tại");

        // Chỉ đánh giá booking của chính mình
        if (booking.CustomerId != user.CustomerId)
            throw new UnauthorizedAccessException(NoPermission);

        // Đã trả phòng thật (do nhân viên xác nhận) mới được đánh giá
        if (booking.Status != BookingStatuses.CheckedOut)
            throw new ArgumentException("Chỉ đánh giá được sau khi đã trả phòng");

        if (await _repo.BookingHasReviewAsync(dto.BookingId))
            throw new ArgumentException("Booking này đã được đánh giá");

        var review = new Review
        {
            BookingId = booking.Id,
            RoomId = booking.RoomId,
            CustomerId = booking.CustomerId,
            Rating = dto.Rating,
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(review);
        await _repo.SaveChangesAsync();

        // Đọc lại để có đủ số phòng, tên khách
        var saved = await _repo.GetByIdAsync(review.Id);
        return ToDto(saved!);
    }

    public async Task UpdateAsync(int id, UpdateReviewDto dto, CurrentUser user)
    {
        var review = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy đánh giá có Id = {id}");

        // Chỉ chủ đánh giá được sửa (Admin không sửa lời người khác)
        if (user.CustomerId != review.CustomerId)
            throw new UnauthorizedAccessException(NoPermission);

        review.Rating = dto.Rating;
        review.Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();

        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, CurrentUser user)
    {
        var review = await _repo.GetForUpdateAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy đánh giá có Id = {id}");

        // Chủ đánh giá, Staff hoặc Admin được xóa
        if (!user.IsAdmin && user.CustomerId != review.CustomerId)
            throw new UnauthorizedAccessException(NoPermission);

        _repo.Remove(review);
        await _repo.SaveChangesAsync();
    }

    private static ReviewDto ToDto(Review x) => new()
    {
        Id = x.Id,
        BookingId = x.BookingId,
        RoomId = x.RoomId,
        RoomNumber = x.Room?.RoomNumber ?? string.Empty,
        CustomerId = x.CustomerId,
        CustomerName = x.Customer?.FullName ?? string.Empty,
        Rating = x.Rating,
        Comment = x.Comment,
        CreatedAt = x.CreatedAt
    };
}