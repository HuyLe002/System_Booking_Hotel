using HotelBookingAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HotelBookingAPI.Repositories;

public interface IBookingLock
{
    // Mở transaction và khóa dòng phòng đến khi commit hoặc rollback
    Task<IDbContextTransaction> BeginRoomLockAsync(int roomId);
    Task<IDbContextTransaction> BeginBookingLockAsync(int bookingId);
    Task<IDbContextTransaction> BeginRoomAndBookingLockAsync(int roomId, int bookingId);
}

public class BookingLock : IBookingLock
{
    private readonly HotelDbContext _context;

    public BookingLock(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<IDbContextTransaction> BeginRoomLockAsync(int roomId)
    {
        var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            // UPDLOCK + HOLDLOCK: ai khóa trước thì giữ đến hết transaction,
            // ai đến sau phải đợi ở dòng này rồi mới đi tiếp
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT Id FROM Rooms WITH (UPDLOCK, HOLDLOCK) WHERE Id = {roomId}");

            return tx;
        }
        catch
        {
            await tx.DisposeAsync();
            throw;
        }
    }

    public async Task<IDbContextTransaction> BeginBookingLockAsync(int bookingId)
    {
        var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT Id FROM Bookings WITH (UPDLOCK, HOLDLOCK) WHERE Id = {bookingId}");

            return tx;
        }
        catch
        {
            await tx.DisposeAsync();
            throw;
        }
    }

    public async Task<IDbContextTransaction> BeginRoomAndBookingLockAsync(int roomId, int bookingId)
    {
        var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT Id FROM Rooms WITH (UPDLOCK, HOLDLOCK) WHERE Id = {roomId}");
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT Id FROM Bookings WITH (UPDLOCK, HOLDLOCK) WHERE Id = {bookingId}");

            return tx;
        }
        catch
        {
            await tx.DisposeAsync();
            throw;
        }
    }
}