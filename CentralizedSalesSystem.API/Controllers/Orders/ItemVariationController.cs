using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Route("itemVariations")]
    public class ItemVariationsController : ControllerBase
    {
        private readonly IItemVariationService _service;

        public ItemVariationsController(IItemVariationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetItemVariations(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 20, 
            [FromQuery] string? sortBy = null, 
            [FromQuery] string? sortDirection = null, 
            [FromQuery] long? filterByItemId = null,
            [FromQuery] string? filterByName = null)
        {
            var result = await _service.GetItemVariationsAsync(page, limit, sortBy, sortDirection, filterByItemId, filterByName);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemVariationResponseDto>> GetItemVariationById(long id)
        {
            var result = await _service.GetItemVariationByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ItemVariationResponseDto>> CreateItemVariation(ItemVariationCreateDto dto)
        {
            var created = await _service.CreateItemVariationAsync(dto);
            return created == null ? BadRequest("Item not found.") : Ok(created);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ItemVariationResponseDto>> UpdateItemVariation(long id, ItemVariationUpdateDto dto)
        {
            var updated = await _service.UpdateItemVariationAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemVariation(long id)
        {
            var deleted = await _service.DeleteItemVariationAsync(id);
            return deleted ? Ok(new { message = "Successfully deleted item variation" }) : NotFound();
        }
    }
}
