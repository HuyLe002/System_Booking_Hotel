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
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT \"Id\" FROM \"Rooms\" WHERE \"Id\" = {roomId} FOR UPDATE");

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
                $"SELECT \"Id\" FROM \"Bookings\" WHERE \"Id\" = {bookingId} FOR UPDATE");

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
                $"SELECT \"Id\" FROM \"Rooms\" WHERE \"Id\" = {roomId} FOR UPDATE");
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT \"Id\" FROM \"Bookings\" WHERE \"Id\" = {bookingId} FOR UPDATE");

            return tx;
        }
        catch
        {
            await tx.DisposeAsync();
            throw;
        }
    }
}