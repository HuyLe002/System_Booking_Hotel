using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelBookingAPI.Data;

public sealed class HotelDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
{
    public HotelDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Set ConnectionStrings__DefaultConnection before running EF Core tools.");

        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new HotelDbContext(options);
    }
}
