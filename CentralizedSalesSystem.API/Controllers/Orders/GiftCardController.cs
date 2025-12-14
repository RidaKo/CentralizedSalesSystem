using CentralizedSalesSystem.API.Models.Orders.DTOs.GiftCardDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralizedSalesSystem.API.Controllers.Orders
{

    [ApiController]
    [Route("giftCards")]
    public class GiftCardsController : ControllerBase
    {
        private readonly IGiftCardService _service;

        public GiftCardsController(IGiftCardService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetGiftCards(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] string? filterByCode = null,
            [FromQuery] PaymentCurrency? filterByCurrency = null,
            [FromQuery] long? filterByIssuedBy = null,
            [FromQuery] string? filterByIssuedTo = null,
            [FromQuery] long? filterByBusinessId = null,
            [FromQuery] GiftCardStatus? filterByStatus = null,
            [FromQuery] DateTimeOffset? filterByIssueDate = null,
            [FromQuery] DateTimeOffset? filterByExpirationDate = null)
        {
            var result = await _service.GetGiftCardsAsync(
                page,
                limit,
                sortBy,
                sortDirection,
                filterByCode,
                filterByCurrency,
                filterByIssuedBy,
                filterByIssuedTo,
                filterByBusinessId,
                filterByStatus,
                filterByIssueDate,
                filterByExpirationDate
            );

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGiftCard(long id)
        {
            var giftCard = await _service.GetGiftCardByIdAsync(id);
            return giftCard == null ? NotFound() : Ok(giftCard);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGiftCard(GiftCardCreateDto dto)
        {
            var created = await _service.CreateGiftCardAsync(dto);
            return CreatedAtAction(nameof(GetGiftCard), new { id = created.Id }, created);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateGiftCard(long id, GiftCardUpdateDto dto)
        {
            var updated = await _service.UpdateGiftCardAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGiftCard(long id)
        {
            var deleted = await _service.DeleteGiftCardAsync(id);
            return deleted ? Ok() : NotFound();
        }
    }
}
