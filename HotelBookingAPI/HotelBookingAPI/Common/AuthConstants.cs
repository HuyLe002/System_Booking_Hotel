namespace HotelBookingAPI.Common;

// Dùng hằng số thay vì gõ chuỗi "Admin" khắp nơi để tránh sai chính tả
public static class Roles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string Customer = "Customer";
}

public static class Policies
{
    public const string StaffOrAdmin = "StaffOrAdmin";
}