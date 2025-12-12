using CentralizedSalesSystem.API.Models.Orders.DTOs.TableDTOs;

namespace CentralizedSalesSystem.API.Services
{
    public interface ITableService
    {
        Task<object> GetAllAsync(int page, int limit, string? sortBy, string? sortDirection,
            string? filterByName, string? filterByStatus, int? filterByCapacity, long? filterByBusinessId);

        Task<TableResponseDto?> GetByIdAsync(long tableId);
        Task<TableResponseDto> CreateAsync(TableCreateDto dto);
        Task<TableResponseDto?> PatchAsync(long tableId, TablePatchDto dto);
        Task<bool> DeleteAsync(long tableId);
    }
}