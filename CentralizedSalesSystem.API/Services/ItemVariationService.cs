using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class ItemVariationService : IItemVariationService
    {
        private readonly CentralizedSalesDbContext _db;

        public ItemVariationService(CentralizedSalesDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetItemVariationsAsync(int page, int limit, string? sortBy = null, string? sortDirection = null, long? filterByItemId = null, string? filterByName = null)
        {
            var query = _db.ItemVariations
                .Include(v => v.Item)
                .Include(v => v.Options)
                .AsQueryable();

            if (filterByItemId.HasValue)
                query = query.Where(v => v.ItemId == filterByItemId.Value);

            if (!string.IsNullOrWhiteSpace(filterByName))
                query = query.Where(d => d.Name.Contains(filterByName));


            bool desc = sortDirection?.ToLower() == "desc";
            query = sortBy switch
            {
                "name" => desc ? query.OrderByDescending(v => v.Name) : query.OrderBy(v => v.Name),
                _ => query.OrderBy(v => v.Id)
            };

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var variations = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var result = variations.Select(v => v.ToDto());

            return new
            {
                data = result,
                page,
                limit,
                total,
                totalPages
            };
        }

        public async Task<ItemVariationResponseDto?> GetItemVariationByIdAsync(long id)
        {
            var variation = await _db.ItemVariations
                .Include(v => v.Item)
                .Include(v => v.Options)
                .FirstOrDefaultAsync(v => v.Id == id);
            return variation?.ToDto();
        }

        public async Task<ItemVariationResponseDto?> CreateItemVariationAsync(ItemVariationCreateDto dto)
        {
            var item = await _db.Items.FindAsync(dto.ItemId);
            if (item == null) return null;

            var variation = new ItemVariation
            {
                Name = dto.Name,
                ItemId = dto.ItemId,
                Selection = dto.Selection
            };

            _db.ItemVariations.Add(variation);
            await _db.SaveChangesAsync();
            return variation.ToDto();
        }

        public async Task<ItemVariationResponseDto?> UpdateItemVariationAsync(long id, ItemVariationUpdateDto dto)
        {
            var variation = await _db.ItemVariations.FindAsync(id);
            if (variation == null) return null;

            if (!string.IsNullOrEmpty(dto.Name)) variation.Name = dto.Name;
            if (dto.ItemId.HasValue) variation.ItemId = dto.ItemId.Value;
            if (dto.Selection.HasValue) variation.Selection = dto.Selection.Value;

            await _db.SaveChangesAsync();
            return variation.ToDto();
        }

        public async Task<bool> DeleteItemVariationAsync(long id)
        {
            var variation = await _db.ItemVariations.FindAsync(id);
            if (variation == null) return false;

            _db.ItemVariations.Remove(variation);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
