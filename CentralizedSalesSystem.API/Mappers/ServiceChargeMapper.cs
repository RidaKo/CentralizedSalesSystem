using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;
using CentralizedSalesSystem.API.Models.Orders;
namespace CentralizedSalesSystem.API.Mappers
{
    public static class ServiceChargeMapper
    {
        public static ServiceChargeResponseDto ToDto(this ServiceCharge sc)
        {
            return new ServiceChargeResponseDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Rate = sc.rate,
                CreatedAt = sc.CreatedAt,
                UpdatedAt = sc.UpdatedAt,
                BusinessId = sc.BusinessId
            };
        }
    }
}