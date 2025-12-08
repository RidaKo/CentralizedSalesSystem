using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.DTOs;

namespace CentralizedSalesSystem.API.Mappers
{
    public static class EntityToDtoMapper
    {
        public static OrderItemResponseDto ToDto(this OrderItem item)
        {
            return new OrderItemResponseDto
            {
                Id = item.Id,
                ItemId = item.ItemId,
                Quantity = item.Quantity,
                DiscountId = item.DiscountId,
                Notes = item.Notes,
                Taxes = item.Taxes?.Select(t => new TaxResponseDto
                {
                    Id = t.Id,
                    BusinessId = t.BusinessId,
                    Name = t.Name,
                    Rate = t.Rate,
                    CreatedAt = t.CreatedAt,
                    Activity = t.Status.ToString().ToLowerInvariant(),
                    EffectiveFrom = t.EffectiveFrom,
                    EffectiveTo = t.EffectiveTo
                }).ToList(),
                ServiceCharges = item.ServiceCharges?.Select(s => new ServiceChargeResponseDto
                {
                    Id = s.Id,
                    BusinessId = s.BusinessId,
                    Name = s.Name,
                    Rate = s.rate,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToList()
            };
        }

        public static ReservationResponseDto ToDto(this CentralizedSalesSystem.API.Models.Reservation r)
        {
            return new ReservationResponseDto
            {
                Id = r.Id,
                BusinessId = r.BusinessId,
                CustomerName = r.CustomerName,
                CustomerPhone = r.CustomerPhone,
                CustomerNote = r.CustomerNote,
                AppointmentTime = r.AppointmentTime,
                CreatedAt = r.CreatedAt,
                CreatedBy = r.CreatedBy,
                Status = r.Status.ToString().ToLowerInvariant(),
                Items = r.Items?.Select(i => i.ToDto()).ToList(),
                AssignedEmployee = r.AssignedEmployee,
                GuestNumber = r.GuestNumber,
                TableId = r.TableId
            };
        }

        public static TableResponseDto ToDto(this Table t)
        {
            return new TableResponseDto
            {
                Id = t.Id,
                BusinessId = t.BusinessId,
                Name = t.Name,
                Capacity = t.Capacity,
                Status = t.Status.ToString().ToLowerInvariant()
            };
        }
    }
}
