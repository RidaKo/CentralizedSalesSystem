using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.GiftCardDTOs;
using CentralizedSalesSystem.API.Mappers;

using CentralizedSalesSystem.API.Models.Orders.enums;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class GiftCardService : IGiftCardService
    {
        private readonly CentralizedSalesDbContext _db;

        public GiftCardService(CentralizedSalesDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetGiftCardsAsync(
    int page,
    int limit,
    string? sortBy = null,
    string? sortDirection = "asc",
    string? filterByCode = null,
    PaymentCurrency? filterByCurrency = null,
    long? filterByIssuedBy = null,
    string? filterByIssuedTo = null,
    long? filterByBusinessId = null,
    GiftCardStatus? filterByStatus = null,
    DateTimeOffset? filterByIssueDate = null,
    DateTimeOffset? filterByExpirationDate = null)
        {
            var query = _db.GiftCards.AsQueryable();

            // ---------- FILTERING ----------
            if (!string.IsNullOrWhiteSpace(filterByCode))
                query = query.Where(g => g.Code.Contains(filterByCode));

            if (filterByCurrency.HasValue)
                query = query.Where(g => g.Currency == filterByCurrency.Value);

            if (filterByIssuedBy.HasValue)
                query = query.Where(g => g.IssuedBy == filterByIssuedBy.Value);

            if (!string.IsNullOrWhiteSpace(filterByIssuedTo))
                query = query.Where(g => g.IssuedTo != null && g.IssuedTo.Contains(filterByIssuedTo));

            if (filterByBusinessId.HasValue)
                query = query.Where(g => g.BusinessId == filterByBusinessId.Value);

            if (filterByStatus.HasValue)
                query = query.Where(g => g.Status == filterByStatus.Value);

            if (filterByIssueDate.HasValue)
                query = query.Where(g => g.IssuedAt >= filterByIssueDate.Value);

            if (filterByExpirationDate.HasValue)
                query = query.Where(g => g.ExpiresAt <= filterByExpirationDate.Value);

            // ---------- SORTING ----------
            query = (sortBy?.ToLower(), sortDirection?.ToLower()) switch
            {
                ("code", "desc") => query.OrderByDescending(g => g.Code),
                ("code", _) => query.OrderBy(g => g.Code),

                ("initialvalue", "desc") => query.OrderByDescending(g => g.InitialValue),
                ("initialvalue", _) => query.OrderBy(g => g.InitialValue),

                ("currentbalance", "desc") => query.OrderByDescending(g => g.CurrentBalance),
                ("currentbalance", _) => query.OrderBy(g => g.CurrentBalance),

                ("issuedat", "desc") => query.OrderByDescending(g => g.IssuedAt),
                ("issuedat", _) => query.OrderBy(g => g.IssuedAt),

                ("expiresat", "desc") => query.OrderByDescending(g => g.ExpiresAt),
                ("expiresat", _) => query.OrderBy(g => g.ExpiresAt),

                ("status", "desc") => query.OrderByDescending(g => g.Status),
                ("status", _) => query.OrderBy(g => g.Status),

                _ => query.OrderBy(g => g.Id)
            };

            // ---------- PAGINATION ----------
            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)limit);

            var giftCards = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // ---------- RESPONSE ----------
            var result = giftCards.Select(g => g.ToDto());

            return new
            {
                data = result,
                page,
                limit,
                total,
                totalPages
            };
        }



        public async Task<GiftCardResponseDto?> GetGiftCardByIdAsync(long id)
        {
            var giftCard = await _db.GiftCards.FindAsync(id);
            return giftCard?.ToDto();
        }

        public async Task<GiftCardResponseDto> CreateGiftCardAsync(GiftCardCreateDto dto)
        {
            var giftCard = new GiftCard
            {
                Code = Guid.NewGuid().ToString("N").ToUpper(),
                InitialValue = dto.InitialValue,
                CurrentBalance = dto.InitialValue,
                Currency = dto.Currency,
                IssuedAt = DateTimeOffset.UtcNow,
                ExpiresAt = dto.ExpiresAt,
                IssuedBy = dto.IssuedBy,
                IssuedTo = dto.IssuedTo,
                Status = GiftCardStatus.Valid,
                BusinessId = dto.BusinessId
            };

            _db.GiftCards.Add(giftCard);
            await _db.SaveChangesAsync();

            return giftCard.ToDto();
        }

        public async Task<GiftCardResponseDto?> UpdateGiftCardAsync(long id, GiftCardUpdateDto dto)
        {
            var giftCard = await _db.GiftCards.FindAsync(id);
            if (giftCard == null) return null;

            if (dto.ExpiresAt.HasValue)
                giftCard.ExpiresAt = dto.ExpiresAt.Value;

            if (dto.Status.HasValue)
                giftCard.Status = dto.Status.Value;

            await _db.SaveChangesAsync();
            return giftCard.ToDto();
        }

        public async Task<bool> DeleteGiftCardAsync(long id)
        {
            var giftCard = await _db.GiftCards.FindAsync(id);
            if (giftCard == null) return false;

            _db.GiftCards.Remove(giftCard);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
