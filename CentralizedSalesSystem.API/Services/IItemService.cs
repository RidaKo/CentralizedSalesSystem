using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;


namespace CentralizedSalesSystem.API.Services
{
    public interface IItemService
    {
        Task<object> GetItemsAsync(
    int page,
    int limit,
    string? sortBy = null,
    string? sortDirection = null,
    long? filterByBusinessId = null,
    string? filterByName = null,
    ItemType? filterByItemType = null,
    decimal? minPrice = null,
    decimal? maxPrice = null);

        Task<ItemResponseDto?> GetItemByIdAsync(long id);
        Task<ItemResponseDto?> CreateItemAsync(ItemCreateDto dto);
        Task<ItemResponseDto?> UpdateItemAsync(long id, ItemUpdateDto dto);
        Task<bool> DeleteItemAsync(long id);
    }
}
