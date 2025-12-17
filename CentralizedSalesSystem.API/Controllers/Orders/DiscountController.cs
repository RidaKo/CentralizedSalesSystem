using CentralizedSalesSystem.API.Authorization;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using CentralizedSalesSystem.API.Services;
using CentralizedSalesSystem.API.Models.Orders.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Authorize]
    [Route("discounts")]
    public class DiscountsController : ControllerBase
    {
        private readonly IDiscountService _service;

        public DiscountsController(IDiscountService service)
        {
            _service = service;
        }

        [HttpGet]
        [AuthorizePermission(nameof(PermissionCode.DISCOUNT_VIEW))]
        public async Task<ActionResult<object>> GetDiscounts(
           [FromQuery] int page = 1,
           [FromQuery] int limit = 20,
           [FromQuery] string? sortBy = null,
           [FromQuery] string? sortDirection = "asc",
           [FromQuery] string? filterByName = null,
           [FromQuery] decimal? filterByRate = null,
           [FromQuery] long? filterByBusinessId = null,
           [FromQuery] DiscountType? filterByDiscountType = null,
           [FromQuery] DiscountStatus? filterByStatus = null,
           [FromQuery] DiscountAppliesTo? filterByAppliesTo = null)
        {
            var result = await _service.GetDiscountsAsync(
                page, limit, sortBy, sortDirection,
                filterByName, filterByRate, filterByBusinessId,
                filterByDiscountType, filterByStatus, filterByAppliesTo
            );

            return Ok(result);
        }


        [HttpGet("{id}")]
        [AuthorizePermission(nameof(PermissionCode.DISCOUNT_VIEW))]
        public async Task<ActionResult<DiscountResponseDto>> GetDiscountById(long id)
        {
            var discount = await _service.GetDiscountByIdAsync(id);
            return discount == null ? NotFound() : Ok(discount);
        }

        [HttpPost]
        [AuthorizePermission(nameof(PermissionCode.DISCOUNT_CREATE))]
        public async Task<ActionResult<DiscountResponseDto>> CreateDiscount(DiscountCreateDto dto)
        {
            var created = await _service.CreateDiscountAsync(dto);
            return created == null ? BadRequest() : Ok(created);
        }

        [HttpPatch("{id}")]
        [AuthorizePermission(nameof(PermissionCode.DISCOUNT_UPDATE))]
        public async Task<ActionResult<DiscountResponseDto>> UpdateDiscount(long id, DiscountUpdateDto dto)
        {
            var updated = await _service.UpdateDiscountAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        [AuthorizePermission(nameof(PermissionCode.DISCOUNT_DELETE))]
        public async Task<IActionResult> DeleteDiscount(long id)
        {
            try
            {
                var deleted = await _service.DeleteDiscountAsync(id);
                return deleted
                    ? Ok(new { message = "Successfully deleted discount" })
                    : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

    }
}
