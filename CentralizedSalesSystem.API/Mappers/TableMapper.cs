using CentralizedSalesSystem.API.Models.Orders.DTOs.TableDTOs;
using CentralizedSalesSystem.API.Models.Orders;
namespace CentralizedSalesSystem.API.Mappers
{
    public static class TableMapper
    {
        public static TableResponseDto ToDto(this Table t)
        {
            return new TableResponseDto
            {
                Id = t.Id,
                BusinessId = t.BusinessId,
                Name = t.Name,
                Capacity = t.Capacity,
                Status = t.Status
            };
        }
    }
}