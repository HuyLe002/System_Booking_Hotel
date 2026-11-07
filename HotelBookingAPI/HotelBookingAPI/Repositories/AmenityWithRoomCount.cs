using HotelBookingAPI.Models;

namespace HotelBookingAPI.Repositories;

public record AmenityWithRoomCount(Amenity Amenity, int TotalRooms);