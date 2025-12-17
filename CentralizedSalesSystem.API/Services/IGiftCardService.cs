using CentralizedSalesSystem.API.Models.Orders.DTOs.GiftCardDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;


namespace CentralizedSalesSystem.API.Services
{
    public interface IGiftCardService
    {
        Task<object> GetGiftCardsAsync(
            int page,
            int limit,
            string? sortBy,
            string? sortDirection,
            string? filterByCode,
            PaymentCurrency? filterByCurrency,
            long? filterByIssuedBy,
            string? filterByIssuedTo,
            long? filterByBusinessId,
            GiftCardStatus? filterByStatus,
            DateTimeOffset? filterByIssueDate,
            DateTimeOffset? filterByExpirationDate);

        Task<GiftCardResponseDto?> GetGiftCardByIdAsync(long id);
        Task<GiftCardResponseDto> CreateGiftCardAsync(GiftCardCreateDto dto);
        Task<GiftCardResponseDto?> UpdateGiftCardAsync(long id, GiftCardUpdateDto dto);
        Task<bool> DeleteGiftCardAsync(long id);
    }
}
