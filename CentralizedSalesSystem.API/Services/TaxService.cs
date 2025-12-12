using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class TaxService : ITaxService
    {
        private readonly CentralizedSalesDbContext _db;

        public TaxService(CentralizedSalesDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetTaxesAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = null,
            string? filterByName = null,
            decimal? filterByRate = null,
            DateTimeOffset? filterByCreationDate = null,
            string? filterByActivity = null,
            DateTimeOffset? filterByEffectiveFrom = null,
            DateTimeOffset? filterByEffectiveTo = null,
            long? filterByBusinessId = null)
        {
            var query = _db.Taxes.AsQueryable();

            // -------- FILTERS --------
            if (!string.IsNullOrEmpty(filterByName))
                query = query.Where(t => t.Name.Contains(filterByName));

            if (filterByRate.HasValue)
                query = query.Where(t => t.Rate == filterByRate.Value);

            if (filterByCreationDate.HasValue)
                query = query.Where(t => t.CreatedAt.Date == filterByCreationDate.Value.Date);

            if (!string.IsNullOrEmpty(filterByActivity))
            {
                bool isActive = filterByActivity.ToLower() == "active";
                query = query.Where(t => (t.Status == TaxStatus.Active) == isActive);
            }

            if (filterByEffectiveFrom.HasValue)
                query = query.Where(t => t.EffectiveFrom >= filterByEffectiveFrom.Value);

            if (filterByEffectiveTo.HasValue)
                query = query.Where(t => t.EffectiveTo <= filterByEffectiveTo.Value);

            if (filterByBusinessId.HasValue)
                query = query.Where(t => t.BusinessId == filterByBusinessId.Value);

            // -------- SORTING --------
            bool desc = sortDirection?.ToLower() == "desc";
            query = sortBy switch
            {
                "name" => desc ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
                "rate" => desc ? query.OrderByDescending(t => t.Rate) : query.OrderBy(t => t.Rate),
                "createdAt" => desc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                "activity" => desc ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                "effectiveFrom" => desc ? query.OrderByDescending(t => t.EffectiveFrom) : query.OrderBy(t => t.EffectiveFrom),
                "effectiveTo" => desc ? query.OrderByDescending(t => t.EffectiveTo) : query.OrderBy(t => t.EffectiveTo),
                _ => query
            };

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var taxes = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
            var result = taxes.Select(t => t.ToDto());

            return new { data = result, page, limit, total, totalPages };
        }

        public async Task<TaxResponseDto?> GetTaxByIdAsync(long id)
        {
            var tax = await _db.Taxes.FindAsync(id);
            return tax?.ToDto();
        }

        public async Task<TaxResponseDto?> CreateTaxAsync(TaxCreateDto dto)
        {
            var tax = new Tax
            {
                Name = dto.Name,
                Rate = dto.Rate,
                CreatedAt = DateTimeOffset.UtcNow,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                Status = dto.Status,
                BusinessId = dto.BusinessId
            };

            _db.Taxes.Add(tax);
            await _db.SaveChangesAsync();
            return tax.ToDto();
        }

        public async Task<TaxResponseDto?> UpdateTaxAsync(long id, TaxUpdateDto dto)
        {
            var tax = await _db.Taxes.FindAsync(id);
            if (tax == null) return null;

            if (dto.Name != null) tax.Name = dto.Name;
            if (dto.Rate.HasValue) tax.Rate = dto.Rate.Value;
            if (dto.EffectiveFrom.HasValue) tax.EffectiveFrom = dto.EffectiveFrom.Value;
            if (dto.EffectiveTo.HasValue) tax.EffectiveTo = dto.EffectiveTo.Value;
            if (dto.Status.HasValue) tax.Status = dto.Status.Value;
            if (dto.BusinessId.HasValue) tax.BusinessId = dto.BusinessId.Value;

            await _db.SaveChangesAsync();
            return tax.ToDto();
        }

        public async Task<bool> DeleteTaxAsync(long id)
        {
            var tax = await _db.Taxes.FindAsync(id);
            if (tax == null) return false;

            _db.Taxes.Remove(tax);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
