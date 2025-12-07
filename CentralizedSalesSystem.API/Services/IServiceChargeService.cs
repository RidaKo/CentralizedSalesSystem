using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;

namespace CentralizedSalesSystem.API.Services
{
    public interface IServiceChargeService
    {
        Task<object> GetServiceChargesAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = "asc",
            string? filterByName = null,
            decimal? filterByRate = null,
            long? filterByBusinessId = null);

        Task<ServiceChargeResponseDto?> GetServiceChargeByIdAsync(long id);
        Task<ServiceChargeResponseDto> CreateServiceChargeAsync(ServiceChargeCreateDto dto);
        Task<ServiceChargeResponseDto?> UpdateServiceChargeAsync(long id, ServiceChargeUpdateDto dto);
        Task<bool> DeleteServiceChargeAsync(long id);
    }
}
