using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.RefundDTOs;

namespace CentralizedSalesSystem.API.Mappers
{
    public static class RefundMapper
    {
        public static RefundResponseDto ToDto(this Refund refund)
        {
            return new RefundResponseDto
            {
                Id = refund.Id,
                Amount = refund.Amount,
                RefundedAt = refund.RefundedAt,
                Reason = refund.Reason,
                RefundMethod = refund.RefundMethod,
                Currency = refund.Currency,
                Status = refund.Status,
                OrderId = refund.OrderId,
            };
        }
    }
}
