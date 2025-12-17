using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Mappers;

using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly CentralizedSalesDbContext _context;

        public DiscountService(CentralizedSalesDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetDiscountsAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = "asc",
            string? filterByName = null,
            decimal? filterByRate = null,
            long? filterByBusinessId = null,
            DiscountType? filterByDiscountType = null,
            DiscountStatus? filterByStatus = null,
            DiscountAppliesTo? filterByAppliesTo = null)
        {
            var query = _context.Discounts.AsQueryable();

            // ---------- FILTERING ----------
            if (!string.IsNullOrWhiteSpace(filterByName))
                query = query.Where(d => d.Name.Contains(filterByName));

            if (filterByRate.HasValue)
                query = query.Where(d => d.rate == filterByRate.Value);

            if (filterByBusinessId.HasValue)
                query = query.Where(d => d.BusinessId == filterByBusinessId.Value);

            if (filterByDiscountType.HasValue)
                query = query.Where(d => d.Type == filterByDiscountType.Value);

            if (filterByStatus.HasValue)
                query = query.Where(d => d.Status == filterByStatus.Value);

            if (filterByAppliesTo.HasValue)
                query = query.Where(d => d.AppliesTo == filterByAppliesTo.Value);

            // ---------- SORTING ----------
            query = (sortBy?.ToLower(), sortDirection?.ToLower()) switch
            {
                ("name", "desc") => query.OrderByDescending(d => d.Name),
                ("name", _) => query.OrderBy(d => d.Name),

                ("rate", "desc") => query.OrderByDescending(d => d.rate),
                ("rate", _) => query.OrderBy(d => d.rate),

                ("createdat", "desc") => query.OrderByDescending(d => d.ValidFrom),
                ("createdat", _) => query.OrderBy(d => d.ValidFrom),

                _ => query.OrderBy(d => d.Id)
            };

            // ---------- PAGINATION ----------
            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)limit);

            var discounts = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // ---------- RESPONSE ----------
            var result = discounts.Select(d => new DiscountResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Rate = d.rate,
                ValidFrom = d.ValidFrom,
                ValidTo = d.ValidTo,
                Type = d.Type,
                AppliesTo = d.AppliesTo,
                Status = d.Status,
                BusinessId = d.BusinessId
            });

            return new
            {
                data = result,
                page,
                limit,
                total,
                totalPages
            };
        }

        public async Task<DiscountResponseDto?> GetDiscountByIdAsync(long id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return null;

            return new DiscountResponseDto
            {
                Id = discount.Id,
                Name = discount.Name,
                Rate = discount.rate,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo,
                Type = discount.Type,
                AppliesTo = discount.AppliesTo,
                Status = discount.Status,
                BusinessId = discount.BusinessId
            };
        }

        public async Task<DiscountResponseDto> CreateDiscountAsync(DiscountCreateDto dto)
        {
            var discount = new Discount
            {
                Name = dto.Name,
                rate = dto.Rate,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                Type = dto.Type,
                AppliesTo = dto.AppliesTo,
                Status = dto.Status,
                BusinessId = dto.BusinessId
            };

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return new DiscountResponseDto
            {
                Id = discount.Id,
                Name = discount.Name,
                Rate = discount.rate,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo,
                Type = discount.Type,
                AppliesTo = discount.AppliesTo,
                Status = discount.Status,
                BusinessId = discount.BusinessId
            };
        }

        public async Task<DiscountResponseDto?> UpdateDiscountAsync(long id, DiscountUpdateDto dto)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return null;

            if (!string.IsNullOrEmpty(dto.Name)) discount.Name = dto.Name;
            if (dto.Rate.HasValue) discount.rate = dto.Rate.Value;
            if (dto.ValidFrom.HasValue) discount.ValidFrom = dto.ValidFrom.Value;
            if (dto.ValidTo.HasValue) discount.ValidTo = dto.ValidTo.Value;
            if (dto.Type.HasValue) discount.Type = dto.Type.Value;
            if (dto.AppliesTo.HasValue) discount.AppliesTo = dto.AppliesTo.Value;
            if (dto.Status.HasValue) discount.Status = dto.Status.Value;

            await _context.SaveChangesAsync();

            return new DiscountResponseDto
            {
                Id = discount.Id,
                Name = discount.Name,
                Rate = discount.rate,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo,
                Type = discount.Type,
                AppliesTo = discount.AppliesTo,
                Status = discount.Status,
                BusinessId = discount.BusinessId
            };
        }

        public async Task<bool> DeleteDiscountAsync(long id)
        {
            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Id == id);
            if (discount == null)
                return false;

            // Check FK constraint
            if (await _context.Orders.AnyAsync(o => o.DiscountId == id))
                throw new InvalidOperationException("Cannot delete discount because it is used by existing orders.");

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
