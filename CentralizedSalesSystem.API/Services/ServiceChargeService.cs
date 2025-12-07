using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;
using CentralizedSalesSystem.API.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class ServiceChargeService : IServiceChargeService
    {
        private readonly CentralizedSalesDbContext _db;

        public ServiceChargeService(CentralizedSalesDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetServiceChargesAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = "asc",
            string? filterByName = null,
            decimal? filterByRate = null,
            long? filterByBusinessId = null)
        {
            var query = _db.ServiceCharges.AsQueryable();

            // -------- FILTERS --------
            if (!string.IsNullOrWhiteSpace(filterByName))
                query = query.Where(sc => sc.Name.Contains(filterByName));

            if (filterByRate.HasValue)
                query = query.Where(sc => sc.rate == filterByRate.Value);

            if (filterByBusinessId.HasValue)
                query = query.Where(sc => sc.BusinessId == filterByBusinessId.Value);

            // -------- SORT --------
            bool desc = sortDirection?.ToLower() == "desc";

            query = sortBy switch
            {
                "name" => desc ? query.OrderByDescending(sc => sc.Name) : query.OrderBy(sc => sc.Name),
                "rate" => desc ? query.OrderByDescending(sc => sc.rate) : query.OrderBy(sc => sc.rate),
                "createdAt" => desc ? query.OrderByDescending(sc => sc.CreatedAt) : query.OrderBy(sc => sc.CreatedAt),
                "updatedAt" => desc ? query.OrderByDescending(sc => sc.UpdatedAt) : query.OrderBy(sc => sc.UpdatedAt),
                _ => query
            };

            // -------- PAGINATION --------
            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var records = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return new
            {
                data = records.Select(sc => sc.ToDto()),
                page,
                limit,
                total,
                totalPages
            };
        }

        public async Task<ServiceChargeResponseDto?> GetServiceChargeByIdAsync(long id)
        {
            var sc = await _db.ServiceCharges.FindAsync(id);
            return sc?.ToDto();
        }

        public async Task<ServiceChargeResponseDto> CreateServiceChargeAsync(ServiceChargeCreateDto dto)
        {
            var sc = new ServiceCharge
            {
                Name = dto.Name,
                rate = dto.Rate,
                BusinessId = dto.BusinessId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.ServiceCharges.Add(sc);
            await _db.SaveChangesAsync();

            return sc.ToDto();
        }

        public async Task<ServiceChargeResponseDto?> UpdateServiceChargeAsync(long id, ServiceChargeUpdateDto dto)
        {
            var sc = await _db.ServiceCharges.FindAsync(id);
            if (sc == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Name)) sc.Name = dto.Name;
            if (dto.Rate.HasValue) sc.rate = dto.Rate.Value;

            sc.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
            return sc.ToDto();
        }

        public async Task<bool> DeleteServiceChargeAsync(long id)
        {
            var sc = await _db.ServiceCharges.FindAsync(id);
            if (sc == null) return false;

            _db.ServiceCharges.Remove(sc);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
