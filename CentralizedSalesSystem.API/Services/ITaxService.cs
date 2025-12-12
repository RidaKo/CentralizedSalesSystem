using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;

namespace CentralizedSalesSystem.API.Services
{
    public interface ITaxService
    {
        Task<object> GetTaxesAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = null,
            string? filterByName = null,
            decimal? filterByRate = null,
            DateTimeOffset? filterByCreationDate = null,
            string? filterByActivity = null,
            DateTimeOffset? filterByEffectiveFrom = null,
            DateTimeOffset? filterByEffectiveTo = null,
            long? filterByBusinessId = null);

        Task<TaxResponseDto?> GetTaxByIdAsync(long id);
        Task<TaxResponseDto?> CreateTaxAsync(TaxCreateDto dto);
        Task<TaxResponseDto?> UpdateTaxAsync(long id, TaxUpdateDto dto);
        Task<bool> DeleteTaxAsync(long id);
    }
}
