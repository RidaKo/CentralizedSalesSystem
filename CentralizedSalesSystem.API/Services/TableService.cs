using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Mappers;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TableDTOs;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class TableService : ITableService
    {
        private readonly CentralizedSalesDbContext _db;

        public TableService(CentralizedSalesDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetAllAsync(int page, int limit, string? sortBy, string? sortDirection,
            string? filterByName, string? filterByStatus, int? filterByCapacity, long? filterByBusinessId)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 20;

            IQueryable<Table> query = _db.Tables.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterByName))
            {
                query = query.Where(t => t.Name.Contains(filterByName));
            }

            if (!string.IsNullOrWhiteSpace(filterByStatus))
            {
                if (Enum.TryParse<TableStatus>(filterByStatus, true, out var statusParsed))
                {
                    query = query.Where(t => t.Status == statusParsed);
                }
            }

            if (filterByCapacity.HasValue)
            {
                query = query.Where(t => t.Capacity == filterByCapacity.Value);
            }

            if (filterByBusinessId.HasValue)
            {
                query = query.Where(t => t.BusinessId == filterByBusinessId.Value);
            }

            bool asc = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            var sortKey = (sortBy ?? "name").ToLower();

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)limit);

            query = sortKey switch
            {
                "capacity" => asc ? query.OrderBy(t => t.Capacity) : query.OrderByDescending(t => t.Capacity),
                "status" => asc ? query.OrderBy(t => t.Status) : query.OrderByDescending(t => t.Status),
                _ => asc ? query.OrderBy(t => t.Name) : query.OrderByDescending(t => t.Name),
            };

            var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var result = new
            {
                data = items.Select(t => t.ToDto()),
                page,
                limit,
                total,
                totalPages
            };

            return result;
        }

        public async Task<TableResponseDto?> GetByIdAsync(long tableId)
        {
            var t = await _db.Tables.FindAsync(tableId);
            if (t == null) return null;
            return t.ToDto();
        }

        public async Task<TableResponseDto> CreateAsync(TableCreateDto dto)
        {
            var table = new Table
            {
                BusinessId = dto.BusinessId,
                Name = dto.Name,
                Capacity = dto.Capacity,
                Status = dto.Status
            };

            _db.Tables.Add(table);
            await _db.SaveChangesAsync();

            return table.ToDto();
        }

        public async Task<TableResponseDto?> PatchAsync(long tableId, TablePatchDto dto)
        {
            var t = await _db.Tables.FindAsync(tableId);
            if (t == null) return null;

            if (dto.BusinessId.HasValue) t.BusinessId = dto.BusinessId.Value;
            if (!string.IsNullOrWhiteSpace(dto.Name)) t.Name = dto.Name;
            if (dto.Capacity.HasValue) t.Capacity = dto.Capacity.Value;
            if (dto.Status.HasValue) t.Status = dto.Status.Value;

            await _db.SaveChangesAsync();

            return t.ToDto();
        }


        public async Task<bool> DeleteAsync(long tableId)
        {
            var t = await _db.Tables.FindAsync(tableId);
            if (t == null) return false;

            _db.Tables.Remove(t);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
