using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Controllers
{
    [ApiController]
    [Route("taxes")]
    public class TaxesController : ControllerBase
    {
        private readonly CentralizedSalesDbContext _context;

        public TaxesController(CentralizedSalesDbContext context)
        {
            _context = context;
        }

        // GET /taxes
        [HttpGet]
        public async Task<ActionResult<object>> GetTaxes(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] string? filterByName = null,
            [FromQuery] decimal? filterByRate = null,
            [FromQuery] DateTimeOffset? filterByCreationDate = null,
            [FromQuery] string? filterByActivity = null,
            [FromQuery] DateTimeOffset? filterByEffectiveFrom = null,
            [FromQuery] DateTimeOffset? filterByEffectiveTo = null,
            [FromQuery] long? filterByBusinessId = null)
        {
            var query = _context.Taxes.AsQueryable();

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

            // -------- PAGINATION --------
            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var taxes = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var result = taxes.Select(t => new TaxResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Rate = t.Rate,
                CreatedAt = t.CreatedAt,
                EffectiveFrom = t.EffectiveFrom,
                EffectiveTo = t.EffectiveTo,
                Status = t.Status,
                BusinessId = t.BusinessId
            });

            return Ok(new { data = result, page, limit, total, totalPages });
        }

        // GET /taxes/{taxId}
        [HttpGet("{taxId}")]
        public async Task<ActionResult<TaxResponseDto>> GetTaxById(long taxId)
        {
            var tax = await _context.Taxes.FindAsync(taxId);
            if (tax == null) return NotFound();

            return Ok(new TaxResponseDto
            {
                Id = tax.Id,
                Name = tax.Name,
                Rate = tax.Rate,
                CreatedAt = tax.CreatedAt,
                EffectiveFrom = tax.EffectiveFrom,
                EffectiveTo = tax.EffectiveTo,
                Status = tax.Status,
                BusinessId = tax.BusinessId
            });
        }

        // POST /taxes
        [HttpPost]
        public async Task<ActionResult<TaxResponseDto>> CreateTax(TaxCreateDto dto)
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

            _context.Taxes.Add(tax);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaxById), new { taxId = tax.Id }, new TaxResponseDto
            {
                Id = tax.Id,
                Name = tax.Name,
                Rate = tax.Rate,
                CreatedAt = tax.CreatedAt,
                EffectiveFrom = tax.EffectiveFrom,
                EffectiveTo = tax.EffectiveTo,
                Status = tax.Status,
                BusinessId = tax.BusinessId
            });
        }

        // PATCH /taxes/{taxId}
        [HttpPatch("{taxId}")]
        public async Task<ActionResult<TaxResponseDto>> ModifyTax(long taxId, TaxUpdateDto dto)
        {
            var tax = await _context.Taxes.FindAsync(taxId);
            if (tax == null) return NotFound();

            if (dto.Name != null) tax.Name = dto.Name;
            if (dto.Rate.HasValue) tax.Rate = dto.Rate.Value;
            if (dto.EffectiveFrom.HasValue) tax.EffectiveFrom = dto.EffectiveFrom.Value;
            if (dto.EffectiveTo.HasValue) tax.EffectiveTo = dto.EffectiveTo;
            if (dto.Status.HasValue) tax.Status = dto.Status.Value;
            if (dto.BusinessId.HasValue) tax.BusinessId = dto.BusinessId.Value;

            await _context.SaveChangesAsync();

            return Ok(new TaxResponseDto
            {
                Id = tax.Id,
                Name = tax.Name,
                Rate = tax.Rate,
                CreatedAt = tax.CreatedAt,
                EffectiveFrom = tax.EffectiveFrom,
                EffectiveTo = tax.EffectiveTo,
                Status = tax.Status,
                BusinessId = tax.BusinessId
            });
        }

        // DELETE /taxes/{taxId}
        [HttpDelete("{taxId}")]
        public async Task<IActionResult> DeleteTax(long taxId)
        {
            var tax = await _context.Taxes.FindAsync(taxId);
            if (tax == null) return NotFound();

            _context.Taxes.Remove(tax);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully deleted tax" });
        }
    }
}
