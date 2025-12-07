using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;


namespace CentralizedSalesSystem.API.Services
{
    public interface IDiscountService
    {
        Task<object> GetDiscountsAsync(
    int page,
    int limit,
    string? sortBy = null,
    string? sortDirection = "asc",
    string? filterByName = null,
    decimal? filterByRate = null,
    long? filterByBusinessId = null,
    DiscountType? filterByDiscountType = null,
    DiscountStatus? filterByStatus = null,
    DiscountAppliesTo? filterByAppliesTo = null);
        Task<DiscountResponseDto?> GetDiscountByIdAsync(long id);
        Task<DiscountResponseDto?> CreateDiscountAsync(DiscountCreateDto dto);
        Task<DiscountResponseDto?> UpdateDiscountAsync(long id, DiscountUpdateDto dto);
        Task<bool> DeleteDiscountAsync(long id);
    }
}
