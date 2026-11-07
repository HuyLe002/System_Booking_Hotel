using HotelBookingAPI.DTOs;

namespace HotelBookingAPI.Services;

public interface IPricingRuleService
{
    Task<List<PricingRuleDto>> GetAllAsync(bool activeOnly);
    Task<PricingRuleDto> GetByIdAsync(int id);
    Task<PricingRuleDto> CreateAsync(CreatePricingRuleDto dto);
    Task UpdateAsync(int id, UpdatePricingRuleDto dto);
    Task DeleteAsync(int id);
    Task<QuoteDto> GetQuoteAsync(int roomId, DateTime checkIn, DateTime checkOut);
}