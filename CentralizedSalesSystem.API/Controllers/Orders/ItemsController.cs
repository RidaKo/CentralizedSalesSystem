using CentralizedSalesSystem.API.Authorization;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralizedSalesSystem.API.Controllers.Items
{
    [ApiController]
    [Authorize]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemsController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet]
        [AuthorizePermission(nameof(PermissionCode.ITEM_VIEW))]
        public async Task<ActionResult<object>> GetItems(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null,
            [FromQuery] long? filterByBusinessId = null,
            [FromQuery] string? filterByName = null,
            [FromQuery] ItemType? filterByItemType = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            var result = await _itemService.GetItemsAsync(
                page, limit, sortBy, sortDirection,
                filterByBusinessId, filterByName,
                filterByItemType, minPrice, maxPrice);

            return Ok(result);
        }


        [HttpGet("{id}")]
        [AuthorizePermission(nameof(PermissionCode.ITEM_VIEW))]
        public async Task<ActionResult<ItemResponseDto>> GetItemById(long id)
        {
            var item = await _itemService.GetItemByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [AuthorizePermission(nameof(PermissionCode.ITEM_CREATE))]
        public async Task<ActionResult<ItemResponseDto>> CreateItem(ItemCreateDto dto)
        {
            var created = await _itemService.CreateItemAsync(dto);
            return Ok(created);
        }

        [HttpPatch("{id}")]
        [AuthorizePermission(nameof(PermissionCode.ITEM_UPDATE))]
        public async Task<ActionResult<ItemResponseDto>> UpdateItem(long id, ItemUpdateDto dto)
        {
            var updated = await _itemService.UpdateItemAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        [AuthorizePermission(nameof(PermissionCode.ITEM_DELETE))]
        public async Task<IActionResult> DeleteItem(long id)
        {
            var deleted = await _itemService.DeleteItemAsync(id);
            return deleted ? Ok(new { message = "Successfully deleted item" }) : NotFound();
        }
    }
}
