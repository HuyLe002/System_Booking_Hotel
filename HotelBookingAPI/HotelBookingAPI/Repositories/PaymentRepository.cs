using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly HotelDbContext _context;

    public PaymentRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetBookingWithPaymentsAsync(int bookingId) =>
        await _context.Bookings
            .AsNoTracking()
            .Include(x => x.Payments)
            .Include(x => x.PaymentClaims)
            .Include(x => x.Room)
                .ThenInclude(x => x!.RoomType)
            .FirstOrDefaultAsync(x => x.Id == bookingId);

    public async Task<Booking?> GetBookingWithPaymentsForUpdateAsync(int bookingId) =>
        await _context.Bookings
            .Include(x => x.Payments)
            .Include(x => x.PaymentClaims)
            .FirstOrDefaultAsync(x => x.Id == bookingId);

    public async Task<(List<PaymentClaim> Items, int TotalItems)> GetPendingClaimsAsync(int page, int pageSize)
    {
        var claims = _context.PaymentClaims
            .AsNoTracking()
            .Where(x => x.Status == PaymentClaimStatuses.Pending);

        var totalItems = await claims.CountAsync();
        var items = await claims
            .OrderBy(x => x.CustomerConfirmedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.Booking!)
                .ThenInclude(x => x.Customer)
            .Include(x => x.Booking!)
                .ThenInclude(x => x.Room!)
                    .ThenInclude(x => x.RoomType)
            .Include(x => x.Booking!)
                .ThenInclude(x => x.Payments)
            .AsSplitQuery()
            .ToListAsync();

        return (items, totalItems);
    }

    public async Task AddAsync(Payment payment) =>
        await _context.Payments.AddAsync(payment);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}