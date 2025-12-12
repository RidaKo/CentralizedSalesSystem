using CentralizedSalesSystem.API.Models.Reservations;

namespace CentralizedSalesSystem.API.Mappers
{
    public static class EntityToDtoMapper
    {
        public static ReservationResponseDto ToDto(this Reservation r)
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
                Status = r.Status,
                Items = r.Items?.Select(i => i.ToDto()).ToList(),
                AssignedEmployee = r.AssignedEmployee,
                GuestNumber = r.GuestNumber,
                TableId = r.TableId
            };
        }
    }
}
