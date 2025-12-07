using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Route("orderItems")]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemsController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] long? filterByOrderId = null,
            [FromQuery] long? filterByItemId = null,
            [FromQuery] long? filterByDiscountId = null
        )
        {
            return Ok(await _orderItemService.GetOrderItemsAsync(
                page, limit, sortBy, sortDirection,
                filterByOrderId, filterByItemId, filterByDiscountId
            ));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItemResponseDto>> GetOrderItemById(long id)
        {
            var item = await _orderItemService.GetOrderItemByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<OrderItemResponseDto>> CreateOrderItem(OrderItemCreateDto dto)
        {
            var created = await _orderItemService.CreateOrderItemAsync(dto);
            if (created == null) return BadRequest("Item or Order not found.");
            return Ok(created);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<OrderItemResponseDto>> UpdateOrderItem(long id, OrderItemUpdateDto dto)
        {
            var updated = await _orderItemService.UpdateOrderItemAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(long id)
        {
            var deleted = await _orderItemService.DeleteOrderItemAsync(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Successfully deleted order item" });
        }
    }
}
