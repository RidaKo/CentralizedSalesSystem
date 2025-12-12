using CentralizedSalesSystem.API.Models.Orders.DTOs.RefundDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;

namespace CentralizedSalesSystem.API.Services
{
    public interface IRefundService
    {
        Task<object> GetRefundsAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = null,
            decimal? filterByAmount = null,
            PaymentMethod? filterByRefundMethod = null,
            DateTimeOffset? filterByRefundDate = null,
            PaymentCurrency? filterByCurrency = null,
            PaymentStatus? filterByStatus = null,
            long? filterByOrderId = null,
            long? filterByRefunderId = null
        );
        Task<RefundResponseDto?> GetRefundByIdAsync(long id);
        Task<RefundResponseDto> CreateRefundAsync(RefundCreateDto dto);
        Task<RefundResponseDto?> UpdateRefundAsync(long id, RefundUpdateDto dto);
        Task<bool> DeleteRefundAsync(long id);
    }
}
