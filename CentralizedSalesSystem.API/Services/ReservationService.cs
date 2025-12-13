using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Mappers;
using CentralizedSalesSystem.API.Models.Reservations;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class ReservationService : IReservationService
    {
        private readonly CentralizedSalesDbContext _db;

        public ReservationService(CentralizedSalesDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetAllAsync(int page, int limit, string? sortBy, string? sortDirection,
            string? filterByName, string? filterByPhone, DateTimeOffset? filterByAppointmentTime,
            DateTimeOffset? filterByCreationTime, string? filterByStatus, long? filterByBusinessId,
            long? filterByUserId, long? filterByTableId)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 20;

            IQueryable<Reservation> query = _db.Reservations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterByName))
            {
                query = query.Where(r => r.CustomerName != null && r.CustomerName.Contains(filterByName));
            }

            if (!string.IsNullOrWhiteSpace(filterByPhone))
            {
                query = query.Where(r => r.CustomerPhone != null && r.CustomerPhone.Contains(filterByPhone));
            }

            if (filterByAppointmentTime.HasValue)
            {
                query = query.Where(r => r.AppointmentTime == filterByAppointmentTime.Value);
            }

            if (filterByCreationTime.HasValue)
            {
                query = query.Where(r => r.CreatedAt == filterByCreationTime.Value);
            }

            if (!string.IsNullOrWhiteSpace(filterByStatus))
            {
                if (Enum.TryParse<ReservationStatus>(filterByStatus, true, out var parsed))
                {
                    query = query.Where(r => r.Status == parsed);
                }
            }

            if (filterByBusinessId.HasValue)
            {
                query = query.Where(r => r.BusinessId == filterByBusinessId.Value);
            }

            if (filterByUserId.HasValue)
            {
                query = query.Where(r => r.CreatedBy == filterByUserId.Value);
            }

            if (filterByTableId.HasValue)
            {
                query = query.Where(r => r.TableId == filterByTableId.Value);
            }

            bool asc = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            var sortKey = (sortBy ?? "createdAt").ToLower();

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)limit);

            query = sortKey switch
            {
                "customername" => asc ? query.OrderBy(r => r.CustomerName) : query.OrderByDescending(r => r.CustomerName),
                "appointmenttime" => asc ? query.OrderBy(r => r.AppointmentTime) : query.OrderByDescending(r => r.AppointmentTime),
                "createdat" => asc ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt),
                "createdby" => asc ? query.OrderBy(r => r.CreatedBy) : query.OrderByDescending(r => r.CreatedBy),
                "status" => asc ? query.OrderBy(r => r.Status) : query.OrderByDescending(r => r.Status),
                "guestnumber" => asc ? query.OrderBy(r => r.GuestNumber) : query.OrderByDescending(r => r.GuestNumber),
                _ => asc ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt),
            };

            var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var result = new
            {
                data = items.Select(r => r.ToDto()),
                page,
                limit,
                total,
                totalPages
            };

            return result;
        }

        public async Task<ReservationResponseDto?> GetByIdAsync(long reservationId)
        {
            var r = await _db.Reservations.FindAsync(reservationId);
            if (r == null) return null;
            return r.ToDto();
        }

        public async Task<ReservationResponseDto> CreateAsync(ReservationCreateDto dto)
        {
            var reservation = new Reservation
            {
                BusinessId = dto.BusinessId,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                CustomerNote = dto.CustomerNote,
                AppointmentTime = dto.AppointmentTime,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = dto.CreatedBy,
                AssignedEmployee = dto.AssignedEmployee,
                GuestNumber = dto.GuestNumber,
                TableId = dto.TableId
            };

            reservation.Status = dto.Status;

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            return reservation.ToDto();
        }

        public async Task<ReservationResponseDto?> PatchAsync(long reservationId, ReservationPatchDto dto)
        {
            var r = await _db.Reservations.FindAsync(reservationId);
            if (r == null) return null;

            if (dto.BusinessId.HasValue) r.BusinessId = dto.BusinessId.Value;
            if (!string.IsNullOrWhiteSpace(dto.CustomerName)) r.CustomerName = dto.CustomerName;
            if (!string.IsNullOrWhiteSpace(dto.CustomerPhone)) r.CustomerPhone = dto.CustomerPhone;
            if (!string.IsNullOrWhiteSpace(dto.CustomerNote)) r.CustomerNote = dto.CustomerNote;
            if (dto.AppointmentTime.HasValue) r.AppointmentTime = dto.AppointmentTime.Value;
            if (dto.CreatedBy.HasValue) r.CreatedBy = dto.CreatedBy.Value;
            if (dto.AssignedEmployee.HasValue) r.AssignedEmployee = dto.AssignedEmployee;
            if (dto.GuestNumber.HasValue) r.GuestNumber = dto.GuestNumber.Value;
            if (dto.TableId.HasValue) r.TableId = dto.TableId;
            if (dto.Status.HasValue) r.Status = dto.Status.Value;

            await _db.SaveChangesAsync();

            return r.ToDto();
        }

        public async Task<bool> DeleteAsync(long reservationId)
        {
            var r = await _db.Reservations.FindAsync(reservationId);
            if (r == null) return false;

            _db.Reservations.Remove(r);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
