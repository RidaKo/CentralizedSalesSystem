using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;

namespace CentralizedSalesSystem.API.Services
{
    public interface IItemVariationService
    {
        Task<object> GetItemVariationsAsync(int page, int limit, string? sortBy = null, string? sortDirection = null, long? filterByItemId = null, string? filterByName = null);
        Task<ItemVariationResponseDto?> GetItemVariationByIdAsync(long id);
        Task<ItemVariationResponseDto?> CreateItemVariationAsync(ItemVariationCreateDto dto);
        Task<ItemVariationResponseDto?> UpdateItemVariationAsync(long id, ItemVariationUpdateDto dto);
        Task<bool> DeleteItemVariationAsync(long id);
    }
}
