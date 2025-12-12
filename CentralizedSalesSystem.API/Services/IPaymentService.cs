using CentralizedSalesSystem.API.Models.Orders.DTOs.PaymentDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;

namespace CentralizedSalesSystem.API.Services
{
    public interface IPaymentService
    {
        Task<object> GetPaymentsAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = null,
            PaymentMethod? filterByMethod = null,
            DateTimeOffset? filterByCreatedAt = null,
            PaymentCurrency? filterByCurrency = null,
            PaymentStatus? filterByStatus = null,
            long? filterByOrderId = null);

        Task<PaymentResponseDto?> GetPaymentByIdAsync(long id);
        Task<PaymentResponseDto> CreatePaymentAsync(PaymentCreateDto dto);
        Task<PaymentResponseDto?> UpdatePaymentAsync(long id, PaymentUpdateDto dto);
        Task<bool> DeletePaymentAsync(long id);
    }
}
