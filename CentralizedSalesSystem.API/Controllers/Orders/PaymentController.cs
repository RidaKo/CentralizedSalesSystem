using CentralizedSalesSystem.API.Authorization;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.PaymentDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Authorize]
    [Route("payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET: /payments
        [HttpGet]
        [AuthorizePermission(nameof(PermissionCode.PAYMENT_VIEW))]
        public async Task<ActionResult<object>> GetPayments(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] PaymentMethod? filterByMethod = null,
            [FromQuery] DateTimeOffset? filterByCreatedAt = null,
            [FromQuery] PaymentCurrency? filterByCurrency = null,
            [FromQuery] PaymentStatus? filterByStatus = null,
            [FromQuery] long? filterByOrderId = null)
  
        {
            var result = await _paymentService.GetPaymentsAsync(
                page,
                limit,
                sortBy,
                sortDirection, 
                filterByMethod,
                filterByCreatedAt,
                filterByCurrency,
                filterByStatus,
                filterByOrderId
            );

            return Ok(result);
        }

        // GET: /payments/{id}
        [HttpGet("{id}")]
        [AuthorizePermission(nameof(PermissionCode.PAYMENT_VIEW))]
        public async Task<ActionResult<PaymentResponseDto>> GetPaymentById(long id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        // POST: /payments
        [HttpPost]
        [AuthorizePermission(nameof(PermissionCode.PAYMENT_CREATE))]
        public async Task<ActionResult<PaymentResponseDto>> CreatePayment(PaymentCreateDto dto)
        {
            var created = await _paymentService.CreatePaymentAsync(dto);
            if (created == null)
                return BadRequest("Order not found.");

            return Ok(created);
        }

        // PATCH: /payments/{id}
        [HttpPatch("{id}")]
        [AuthorizePermission(nameof(PermissionCode.PAYMENT_UPDATE))]
        public async Task<ActionResult<PaymentResponseDto>> UpdatePayment(long id, PaymentUpdateDto dto)
        {
            var updated = await _paymentService.UpdatePaymentAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE: /payments/{id}
        [HttpDelete("{id}")]
        [AuthorizePermission(nameof(PermissionCode.PAYMENT_DELETE))]
        public async Task<IActionResult> DeletePayment(long id)
        {
            var deleted = await _paymentService.DeletePaymentAsync(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Successfully deleted payment" });
        }
    }
}
