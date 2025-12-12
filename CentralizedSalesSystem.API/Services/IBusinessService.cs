using CentralizedSalesSystem.API.Models.Business;
using CentralizedSalesSystem.API.Models.Business.DTOs;

namespace CentralizedSalesSystem.API.Services;

public interface IBusinessService
{
    Task<object> GetAllAsync(int page, int limit, string? sortBy, string? sortDirection,
        string? filterByName, string? filterByPhone, string? filterByAddress, string? filterByEmail,
        Currency? filterByCurrency, SubscriptionPlan? filterBySubscriptionPlan, long? filterByOwnerId);

    Task<BusinessResponseDto?> GetByIdAsync(long id);
    Task<BusinessResponseDto> CreateAsync(BusinessCreateDto dto);
    Task<BusinessResponseDto?> PatchAsync(long id, BusinessPatchDto dto);
    Task<bool> DeleteAsync(long id);
}
