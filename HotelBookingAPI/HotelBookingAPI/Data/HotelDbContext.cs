using HotelBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ExtraService> ExtraServices => Set<ExtraService>();
    public DbSet<BookingExtraService> BookingExtraServices => Set<BookingExtraService>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentClaim> PaymentClaims => Set<PaymentClaim>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RoomType>()
            .Property(x => x.PricePerNight).HasPrecision(18, 2);

        // Unique chỉ áp dụng cho dòng CHƯA xóa, để xóa rồi vẫn tạo lại được cùng số phòng / email
        modelBuilder.Entity<Room>()
            .HasIndex(x => x.RoomNumber)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        modelBuilder.Entity<Customer>()
            .HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Username).IsUnique();

        // Refresh token: tra cứu theo băm nên băm phải duy nhất.
        // Xóa tài khoản thì xóa luôn các refresh token của tài khoản đó
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.Property(x => x.TokenHash).HasMaxLength(100);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => x.UserId);

            e.HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Global query filter: mọi truy vấn tự động bỏ qua dòng đã xóa mềm
        modelBuilder.Entity<Room>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(x => !x.IsDeleted);

        // Booking: tiền, mã giảm giá (xóa mã không kéo theo xóa booking) và filter xóa mềm
        modelBuilder.Entity<Booking>(e =>
        {
            e.Property(x => x.TotalPrice).HasPrecision(18, 2);
            e.Property(x => x.DiscountAmount).HasPrecision(18, 2);

            e.HasOne(x => x.Coupon).WithMany()
                .HasForeignKey(x => x.CouponId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(b => !b.Room!.IsDeleted && !b.Customer!.IsDeleted);
        });

        // Sổ giao dịch: Restrict để xóa booking không kéo theo mất lịch sử tiền
        modelBuilder.Entity<Payment>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Type).HasMaxLength(10);
            e.Property(x => x.Method).HasMaxLength(20);
            e.Property(x => x.Note).HasMaxLength(500);
            e.HasIndex(x => x.BookingId);

            e.HasOne(x => x.Booking).WithMany(b => b.Payments)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => !x.Booking!.Room!.IsDeleted && !x.Booking!.Customer!.IsDeleted);
        });

        modelBuilder.Entity<PaymentClaim>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Method).HasMaxLength(20);
            e.Property(x => x.Note).HasMaxLength(500);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.CashierNote).HasMaxLength(500);
            e.HasIndex(x => x.BookingId);

            e.HasOne(x => x.Booking).WithMany(b => b.PaymentClaims)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => !x.Booking!.Room!.IsDeleted && !x.Booking!.Customer!.IsDeleted);
        });

        // Mã giảm giá: Code duy nhất, RowVersion chống hai người cùng lấy lượt cuối
        modelBuilder.Entity<Coupon>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.DiscountType).HasMaxLength(10);
            e.Property(x => x.Value).HasPrecision(18, 2);
            e.Property(x => x.MaxDiscount).HasPrecision(18, 2);
            e.Property(x => x.MinBookingAmount).HasPrecision(18, 2);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => x.Code).IsUnique();
        });

        // Quy tắc giá: RoomTypeId có thể null (áp cho mọi loại phòng).
        // Xóa loại phòng thì xóa luôn các quy tắc riêng của loại đó
        modelBuilder.Entity<PricingRule>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.RuleType).HasMaxLength(10);
            e.Property(x => x.Percent).HasPrecision(5, 2);
            e.HasIndex(x => x.Name).IsUnique();

            e.HasOne(x => x.RoomType).WithMany()
                .HasForeignKey(x => x.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Review: mỗi Booking tối đa một Review (unique index trên BookingId)
        modelBuilder.Entity<Review>(e =>
        {
            e.HasIndex(x => x.BookingId).IsUnique();
            e.HasIndex(x => x.RoomId);
            e.Property(x => x.Comment).HasMaxLength(1000);

            e.HasOne(x => x.Booking).WithOne()
                .HasForeignKey<Review>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Room).WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Customer).WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => !x.Room!.IsDeleted && !x.Customer!.IsDeleted);
        });

        // Danh mục dịch vụ
        modelBuilder.Entity<ExtraService>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Unit).HasMaxLength(30);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.HasIndex(x => x.Name).IsUnique();
        });

        // Dịch vụ của booking: khóa chính kép, xóa booking thì xóa luôn các dòng này,
        // còn dịch vụ đã được dùng thì không cho xóa (Restrict)
        modelBuilder.Entity<BookingExtraService>(e =>
        {
            e.HasKey(x => new { x.BookingId, x.ExtraServiceId });
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);

            e.HasOne(x => x.Booking).WithMany(b => b.ExtraServices)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ExtraService).WithMany()
                .HasForeignKey(x => x.ExtraServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => !x.Booking!.Room!.IsDeleted && !x.Booking!.Customer!.IsDeleted);
        });

        // Giá trị mặc định cho các dòng cũ khi thêm cột Floor
        modelBuilder.Entity<Room>()
            .Property(x => x.Floor).HasDefaultValue(1);

        modelBuilder.Entity<Room>()
            .Property(x => x.Description).HasMaxLength(500);

        modelBuilder.Entity<Room>()
            .Property(x => x.ImageUrl).HasMaxLength(500);

        // Quan hệ n-n: Room <-> Amenity (kèm seed bảng trung gian)
        modelBuilder.Entity<Room>()
            .HasMany(r => r.Amenities)
            .WithMany(a => a.Rooms)
            .UsingEntity(j => j.HasData(
                new { RoomsId = 1, AmenitiesId = 1 },
                new { RoomsId = 1, AmenitiesId = 2 },
                new { RoomsId = 2, AmenitiesId = 1 },
                new { RoomsId = 2, AmenitiesId = 2 },
                new { RoomsId = 2, AmenitiesId = 3 },
                new { RoomsId = 3, AmenitiesId = 1 },
                new { RoomsId = 3, AmenitiesId = 2 },
                new { RoomsId = 3, AmenitiesId = 3 },
                new { RoomsId = 3, AmenitiesId = 4 }
            ));

        // Seed data
        modelBuilder.Entity<RoomType>().HasData(
            new RoomType { Id = 1, Name = "Standard", Description = "Phòng tiêu chuẩn", PricePerNight = 500000, Capacity = 2 },
            new RoomType { Id = 2, Name = "Deluxe", Description = "Phòng cao cấp", PricePerNight = 900000, Capacity = 3 },
            new RoomType { Id = 3, Name = "Suite", Description = "Phòng suite", PricePerNight = 1500000, Capacity = 4 }
        );

        modelBuilder.Entity<Amenity>().HasData(
            new Amenity { Id = 1, Name = "WiFi" },
            new Amenity { Id = 2, Name = "Máy lạnh" },
            new Amenity { Id = 3, Name = "TV" },
            new Amenity { Id = 4, Name = "Bồn tắm" }
        );

        modelBuilder.Entity<Room>().HasData(
            new Room { Id = 1, RoomNumber = "101", Status = "Available", RoomTypeId = 1, Floor = 1, Description = "Phòng tiêu chuẩn tầng 1, hướng sân vườn" },
            new Room { Id = 2, RoomNumber = "201", Status = "Available", RoomTypeId = 2, Floor = 2, Description = "Phòng cao cấp tầng 2, có ban công" },
            new Room { Id = 3, RoomNumber = "301", Status = "Available", RoomTypeId = 3, Floor = 3, Description = "Phòng suite tầng 3, nhìn toàn cảnh thành phố" }
        );

        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, FullName = "Nguyen Van A", Email = "a@gmail.com", Phone = "0900000001" },
            new Customer { Id = 2, FullName = "Tran Thi B", Email = "b@gmail.com", Phone = "0900000002" }
        );

        modelBuilder.Entity<ExtraService>().HasData(
            new ExtraService { Id = 1, Name = "Ăn sáng", Price = 80000, Unit = "suất", IsActive = true },
            new ExtraService { Id = 2, Name = "Đưa đón sân bay", Price = 250000, Unit = "lượt", IsActive = true },
            new ExtraService { Id = 3, Name = "Giặt ủi", Price = 30000, Unit = "kg", IsActive = true }
        );
    }
}