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
            string? sortBy,
            string? sortDirection,
            string? filterByCode,
            PaymentCurrency? filterByCurrency,
            long? filterByIssuedBy,
            string? filterByIssuedTo,
            long? filterByBusinessId,
            GiftCardStatus? filterByStatus,
            DateTimeOffset? filterByIssueDate,
            DateTimeOffset? filterByExpirationDate)
        {
            var query = _db.GiftCards.AsQueryable();

            if (!string.IsNullOrEmpty(filterByCode))
                query = query.Where(g => g.Code.Contains(filterByCode));

            if (filterByCurrency.HasValue)
                query = query.Where(g => g.Currency == filterByCurrency);

            if (filterByIssuedBy.HasValue)
                query = query.Where(g => g.IssuedBy == filterByIssuedBy);

            if (!string.IsNullOrEmpty(filterByIssuedTo))
                query = query.Where(g => g.IssuedTo!.Contains(filterByIssuedTo));

            if (filterByBusinessId.HasValue)
                query = query.Where(g => g.BusinessId == filterByBusinessId);

            if (filterByStatus.HasValue)
                query = query.Where(g => g.Status == filterByStatus);

            if (filterByIssueDate.HasValue)
                query = query.Where(g => g.IssuedAt >= filterByIssueDate);

            if (filterByExpirationDate.HasValue)
                query = query.Where(g => g.ExpiresAt <= filterByExpirationDate);

            bool desc = sortDirection == "desc";
            query = sortBy switch
            {
                "initialValue" => desc ? query.OrderByDescending(g => g.InitialValue) : query.OrderBy(g => g.InitialValue),
                "currentBalance" => desc ? query.OrderByDescending(g => g.CurrentBalance) : query.OrderBy(g => g.CurrentBalance),
                "issuedAt" => desc ? query.OrderByDescending(g => g.IssuedAt) : query.OrderBy(g => g.IssuedAt),
                "expiresAt" => desc ? query.OrderByDescending(g => g.ExpiresAt) : query.OrderBy(g => g.ExpiresAt),
                _ => query
            };

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(g => g.ToDto())
                .ToListAsync();

            return new
            {
                data,
                page,
                limit,
                total,
                totalPages = (int)Math.Ceiling(total / (double)limit)
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
