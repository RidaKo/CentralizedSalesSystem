using CentralizedSalesSystem.API.Models.Orders.DTOs.PaymentDTOs;
using CentralizedSalesSystem.API.Models.Orders;
namespace CentralizedSalesSystem.API.Mappers
{
    public static class PaymentMapper
    {
        public static PaymentResponseDto ToDto(this Payment payment)
        {
            return new PaymentResponseDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaidAt = payment.PaidAt,

                Method = payment.Method,
                Provider = payment.Provider,
                Currency = payment.Currency,
                Status = payment.Status,

                OrderId = payment.OrderId,
                BussinesId = payment.BussinesId,
            };
        }
    }
}