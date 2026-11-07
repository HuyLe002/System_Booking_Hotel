using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public record CustomerWithBookingCount(Customer Customer, int TotalBookings);