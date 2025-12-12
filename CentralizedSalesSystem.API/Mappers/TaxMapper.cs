
using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;
using CentralizedSalesSystem.API.Models.Orders;
namespace CentralizedSalesSystem.API.Mappers
{
    public static class TaxMapper
    {
        public static TaxResponseDto ToDto(this Tax tax)
        {
            return new TaxResponseDto
            {
                Id = tax.Id,
                Name = tax.Name,
                Rate = tax.Rate,
                CreatedAt = tax.CreatedAt,
                EffectiveFrom = tax.EffectiveFrom,
                EffectiveTo = tax.EffectiveTo,
                Status = tax.Status,
                BusinessId = tax.BusinessId
            };
        }
    }
}