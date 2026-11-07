namespace HotelBookingAPI.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer"; // Admin / Staff / Customer
    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }
}