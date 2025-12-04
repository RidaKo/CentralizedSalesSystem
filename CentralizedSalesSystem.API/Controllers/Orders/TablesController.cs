using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TableDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Route("tables")]
    public class TablesController : ControllerBase
    {
        private readonly CentralizedSalesDbContext _context;

        public TablesController(CentralizedSalesDbContext context)
        {
            _context = context;
        }

        // GET: /tables
        [HttpGet]
        public async Task<ActionResult<object>> GetTables(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] string? filterByName = null,
            [FromQuery] string? filterByStatus = null,
            [FromQuery] int? filterByCapacity = null,
            [FromQuery] long? filterByBusinessId = null
        )
        {
            var query = _context.Tables.AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(filterByName))
                query = query.Where(t => t.Name.Contains(filterByName));

            if (!string.IsNullOrEmpty(filterByStatus) &&
                Enum.TryParse<TableStatus>(filterByStatus, true, out var parsedStatus))
            {
                query = query.Where(t => t.Status == parsedStatus);
            }

            if (filterByCapacity.HasValue)
                query = query.Where(t => t.Capacity == filterByCapacity.Value);

            if (filterByBusinessId.HasValue)
                query = query.Where(t => t.BusinessId == filterByBusinessId.Value);

            // Sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                bool descending = sortDirection?.ToLower() == "desc";

                query = sortBy switch
                {
                    "name" => descending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
                    "capacity" => descending ? query.OrderByDescending(t => t.Capacity) : query.OrderBy(t => t.Capacity),
                    "status" => descending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                    _ => query
                };
            }

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var tables = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var result = tables.Select(t => new TableReadDto
            {
                Id = t.Id,
                Name = t.Name,
                Capacity = t.Capacity,
                Status = t.Status,
                BusinessId = t.BusinessId
            });

            return Ok(new
            {
                data = result,
                page,
                limit,
                total,
                totalPages
            });
        }

        // GET: /tables/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TableReadDto>> GetTableById(long id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return NotFound();

            return Ok(new TableReadDto
            {
                Id = table.Id,
                Name = table.Name,
                Capacity = table.Capacity,
                Status = table.Status,
                BusinessId = table.BusinessId
            });
        }

        // POST: /tables
        [HttpPost]
        public async Task<ActionResult<TableReadDto>> CreateTable(TableCreateDto dto)
        {
            var table = new Table
            {
                Name = dto.Name,
                Capacity = dto.Capacity,
                Status = dto.Status,
                BusinessId = dto.BusinessId
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTableById), new { id = table.Id },
                new TableReadDto
                {
                    Id = table.Id,
                    Name = table.Name,
                    Capacity = table.Capacity,
                    Status = table.Status,
                    BusinessId = table.BusinessId
                });
        }

        // PATCH: /tables/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<TableReadDto>> UpdateTable(long id, TableUpdateDto dto)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.Name)) table.Name = dto.Name;
            if (dto.Capacity.HasValue) table.Capacity = dto.Capacity.Value;
            if (dto.Status.HasValue) table.Status = dto.Status.Value;

            await _context.SaveChangesAsync();

            return Ok(new TableReadDto
            {
                Id = table.Id,
                Name = table.Name,
                Capacity = table.Capacity,
                Status = table.Status,
                BusinessId = table.BusinessId
            });
        }

        // DELETE: /tables/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(long id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return NotFound();

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully deleted table" });
        }
    }
}
