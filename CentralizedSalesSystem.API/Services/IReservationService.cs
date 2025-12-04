using CentralizedSalesSystem.API.Models.DTOs;

namespace CentralizedSalesSystem.API.Services
{
    public interface IReservationService
    {
        Task<object> GetAllAsync(int page, int limit, string? sortBy, string? sortDirection,
            string? filterByName, string? filterByPhone, DateTimeOffset? filterByAppointmentTime,
            DateTimeOffset? filterByCreationTime, string? filterByStatus, long? filterByBusinessId,
            long? filterByUserId, long? filterByTableId);

        Task<ReservationResponseDto?> GetByIdAsync(long reservationId);
        Task<ReservationResponseDto> CreateAsync(ReservationCreateDto dto);
        Task<ReservationResponseDto?> PatchAsync(long reservationId, ReservationPatchDto dto);
        Task<bool> DeleteAsync(long reservationId);
    }
}