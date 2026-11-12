using HotelBookingAPI.Data;
using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Tests;

public class PostgreSqlModelTests
{
    [Fact]
    public void BookingNightStatisticsTranslateForPostgreSql()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseNpgsql("Host=localhost;Database=postgres;Username=postgres")
            .Options;

        using var context = new HotelDbContext(options);

        var query = context.Bookings
            .GroupBy(booking => booking.RoomId)
            .Select(group => group.Sum(booking =>
                (int)(booking.CheckOutDate - booking.CheckInDate).TotalDays));

        Assert.Contains("date_part", query.ToQueryString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BookingDatesUsePostgreSqlTimestampWithoutTimeZone()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseNpgsql("Host=localhost;Database=postgres;Username=postgres")
            .Options;

        using var context = new HotelDbContext(options);

        var property = context.Model.FindEntityType(typeof(Booking))!
            .FindProperty(nameof(Booking.CheckInDate));

        Assert.Equal("timestamp without time zone", property!.GetColumnType());
    }
}
