using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Mappers;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.enums;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class ItemService : IItemService
    {
        private readonly CentralizedSalesDbContext _db;

        public ItemService(CentralizedSalesDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetItemsAsync(
    int page,
    int limit,
    string? sortBy = null,
    string? sortDirection = null,
    long? filterByBusinessId = null,
    string? filterByName = null,
    ItemType? filterByItemType = null,
    decimal? minPrice = null,
    decimal? maxPrice = null)
        {
            var query = _db.Items
                .Include(i => i.Variations)
                .Include(i => i.AssociatedRoles)
                .AsQueryable();

            if (filterByBusinessId.HasValue)
                query = query.Where(i => i.BusinessId == filterByBusinessId.Value);

            if (!string.IsNullOrEmpty(filterByName))
                query = query.Where(i => i.Name.Contains(filterByName));

            if (filterByItemType.HasValue)
                query = query.Where(i => i.Type == filterByItemType.Value);

            if (minPrice.HasValue)
                query = query.Where(i => i.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(i => i.Price <= maxPrice.Value);

            bool desc = sortDirection?.ToLower() == "desc";

            query = sortBy switch
            {
                "price" => desc ? query.OrderByDescending(i => i.Price) : query.OrderBy(i => i.Price),
                "type" => desc ? query.OrderByDescending(i => i.Type) : query.OrderBy(i => i.Type),
                "name" => desc ? query.OrderByDescending(i => i.Name) : query.OrderBy(i => i.Name),
                "stock" => desc ? query.OrderByDescending(i => i.Stock) : query.OrderBy(i => i.Stock),
                _ => query.OrderBy(i => i.Id)
            };

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
            var result = items.Select(i => i.ToDto());

            return new
            {
                data = result,
                page,
                limit,
                total,
                totalPages
            };
        }


        public async Task<ItemResponseDto?> GetItemByIdAsync(long id)
        {
            var item = await _db.Items
                .Include(i => i.Variations)
                .FirstOrDefaultAsync(i => i.Id == id);

            return item?.ToDto();
        }

        public async Task<ItemResponseDto?> CreateItemAsync(ItemCreateDto dto)
        {
            if (dto.TaxId.HasValue && dto.TaxId.Value > 0)
            {
                var tax = await _db.Taxes.FindAsync(dto.TaxId.Value);
                if (tax == null || tax.BusinessId != dto.BusinessId)
                    throw new Exception($"Tax {dto.TaxId} is not available for this business");
            }

            var item = new Item
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                Type = dto.Type,
                BusinessId = dto.BusinessId,
                TaxId = dto.TaxId.HasValue && dto.TaxId.Value > 0 ? dto.TaxId.Value : null
            };

            _db.Items.Add(item);
            await _db.SaveChangesAsync();

            return item.ToDto();
        }

        public async Task<ItemResponseDto?> UpdateItemAsync(long id, ItemUpdateDto dto)
        {
            var item = await _db.Items.FindAsync(id);
            if (item == null) return null;

            if (dto.BusinessId.HasValue) item.BusinessId = dto.BusinessId.Value;

            if (!string.IsNullOrEmpty(dto.Name)) item.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) item.Description = dto.Description;
            if (dto.Price.HasValue) item.Price = dto.Price.Value;
            if (dto.Stock.HasValue) item.Stock = dto.Stock.Value;
            if (dto.Type.HasValue) item.Type = dto.Type.Value;
            if (dto.TaxId.HasValue)
            {
                if (dto.TaxId.Value == 0)
                {
                    item.TaxId = null;
                }
                else
                {
                    var tax = await _db.Taxes.FindAsync(dto.TaxId.Value);
                    if (tax == null || tax.BusinessId != item.BusinessId)
                        throw new Exception($"Tax {dto.TaxId} is not available for this business");

                    item.TaxId = tax.Id;
                }
            }

            await _db.SaveChangesAsync();

            return item.ToDto();
        }

        public async Task<bool> DeleteItemAsync(long id)
        {
            var item = await _db.Items.FindAsync(id);
            if (item == null) return false;

            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
