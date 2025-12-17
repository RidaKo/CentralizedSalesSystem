using CentralizedSalesSystem.API.Authorization;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderDTOs;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Authorize]
    [Route("orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: /orders
        [HttpGet]
        [AuthorizePermission(nameof(PermissionCode.ORDER_VIEW))]
        public async Task<ActionResult<object>> GetOrders(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] string? filterByStatus = null,
            [FromQuery] DateTimeOffset? filterByUpdatedAt = null,
            [FromQuery] long? filterByBusinessId = null,
            [FromQuery] long? filterByReservationId = null,
            [FromQuery] long? filterByTableId = null)
        {
            var result = await _orderService.GetOrdersAsync(
                page, limit, sortBy, sortDirection,
                filterByStatus, filterByUpdatedAt,
                filterByBusinessId, filterByReservationId, filterByTableId
            );

            return Ok(result);
        }

        // GET: /orders/{id}
        [HttpGet("{id}")]
        [AuthorizePermission(nameof(PermissionCode.ORDER_VIEW))]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById(long id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        // POST: /orders
        [HttpPost]
        [AuthorizePermission(nameof(PermissionCode.ORDER_CREATE))]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder(OrderCreateDto dto)
        {
            var created = await _orderService.CreateOrderAsync(dto);
            if (created == null) return BadRequest("User not found.");
            return Ok(created);
        }

        // PATCH: /orders/{id}
        [HttpPatch("{id}")]
        [AuthorizePermission(nameof(PermissionCode.ORDER_UPDATE))]
        public async Task<ActionResult<OrderResponseDto>> UpdateOrder(long id, OrderUpdateDto dto)
        {
            var updated = await _orderService.UpdateOrderAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE: /orders/{id}
        [HttpDelete("{id}")]
        [AuthorizePermission(nameof(PermissionCode.ORDER_DELETE))]
        public async Task<IActionResult> DeleteOrder(long id)
        {
            var deleted = await _orderService.DeleteOrderAsync(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Successfully deleted order" });
        }
    }
}
