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
        public async Task<IActionResult> GetGiftCards([FromQuery] int page = 1, [FromQuery] int limit = 20)
            => Ok(await _service.GetGiftCardsAsync(page, limit, null, null, null, null, null, null, null, null, null, null));

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
