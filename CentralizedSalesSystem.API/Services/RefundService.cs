using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Mappers;

using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.RefundDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class RefundService : IRefundService
    {
        private readonly CentralizedSalesDbContext _db;
        private readonly IOrderService _orderService;

        public RefundService(CentralizedSalesDbContext db, IOrderService orderService)
        {
            _db = db;
            _orderService = orderService;
        }

        // -----------------------------
        // GET ALL REFUNDS
        // -----------------------------
        public async Task<object> GetRefundsAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = null,
            decimal? filterByAmount = null,
            PaymentMethod? filterByRefundMethod = null,
            DateTimeOffset? filterByRefundDate = null,
            PaymentCurrency? filterByCurrency = null,
            PaymentStatus? filterByStatus = null,
            long? filterByOrderId = null,
            long? filterByRefunderId = null
        )
        {
            var query = _db.Refunds
                .Include(r => r.Order)
                .AsQueryable();

            // Filters
            if (filterByAmount.HasValue) query = query.Where(r => r.Amount == filterByAmount.Value);
            if (filterByRefundMethod.HasValue) query = query.Where(r => r.RefundMethod == filterByRefundMethod.Value);
            if (filterByRefundDate.HasValue) query = query.Where(r => r.RefundedAt >= filterByRefundDate.Value);
            if (filterByCurrency.HasValue) query = query.Where(r => r.Currency == filterByCurrency.Value);
            if (filterByStatus.HasValue) query = query.Where(r => r.Status == filterByStatus.Value);
            if (filterByOrderId.HasValue) query = query.Where(r => r.OrderId == filterByOrderId.Value);

            // Sorting
            bool desc = sortDirection?.ToLower() == "desc";
            query = sortBy switch
            {
                "amount" => desc ? query.OrderByDescending(r => r.Amount) : query.OrderBy(r => r.Amount),
                "refundedAt" => desc ? query.OrderByDescending(r => r.RefundedAt) : query.OrderBy(r => r.RefundedAt),
                "status" => desc ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
                "currency" => desc ? query.OrderByDescending(r => r.Currency) : query.OrderBy(r => r.Currency),
                "refundMethod" => desc ? query.OrderByDescending(r => r.RefundMethod) : query.OrderBy(r => r.RefundMethod),
                _ => query
            };

            // Pagination
            var total = await query.CountAsync();
            var refunds = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var result = refunds.Select(r => r.ToDto()).ToList();

            return new
            {
                data = result,
                page,
                limit,
                total,
                totalPages = (int)Math.Ceiling(total / (double)limit)
            };
        }

        // -----------------------------
        // GET REFUND BY ID
        // -----------------------------
        public async Task<RefundResponseDto?> GetRefundByIdAsync(long id)
        {
            var refund = await _db.Refunds
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refund == null) return null;

            return refund.ToDto();
        }

        // -----------------------------
        // CREATE REFUND
        // -----------------------------
        public async Task<RefundResponseDto> CreateRefundAsync(RefundCreateDto dto)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                throw new Exception($"Order {dto.OrderId} not found");

            // Calculate remaining amount on order
            decimal amountPaid = order.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);

            decimal alreadyRefunded = (await _db.Refunds
                .Where(r => r.OrderId == dto.OrderId && r.Status == PaymentStatus.Completed)
                .SumAsync(r => r.Amount));

            decimal maxRefundable = Math.Max(0, amountPaid - alreadyRefunded);
            if (dto.Amount > maxRefundable)
                throw new Exception($"Refund amount cannot exceed remaining paid amount: {maxRefundable}");

            var refund = new Refund
            {
                OrderId = dto.OrderId,
                Amount = dto.Amount,
                RefundedAt = DateTimeOffset.UtcNow,
                Reason = dto.Reason,
                RefundMethod = dto.RefundMethod,
                Currency = dto.Currency,
                Status = dto.Status,
            };

            _db.Refunds.Add(refund);
            await _db.SaveChangesAsync();

            // If refund is completed, mark order as Refunded
            if (refund.Status == PaymentStatus.Completed)
            {
                order.Status = OrderStatus.Refunded;
                await _db.SaveChangesAsync();
            }

            return refund.ToDto();
        }

        // -----------------------------
        // UPDATE REFUND
        // -----------------------------
        public async Task<RefundResponseDto?> UpdateRefundAsync(long id, RefundUpdateDto dto)
        {
            var refund = await _db.Refunds.Include(r => r.Order).FirstOrDefaultAsync(r => r.Id == id);
            if (refund == null) return null;

            if (dto.Amount.HasValue) refund.Amount = dto.Amount.Value;
            if (dto.RefundedAt.HasValue) refund.RefundedAt = dto.RefundedAt.Value;
            if (dto.Reason != null) refund.Reason = dto.Reason;
            if (dto.RefundMethod.HasValue) refund.RefundMethod = dto.RefundMethod.Value;
            if (dto.Currency.HasValue) refund.Currency = dto.Currency.Value;
            if (dto.Status.HasValue) refund.Status = dto.Status.Value;

            await _db.SaveChangesAsync();

            // If refund is now completed, mark order as Refunded
            if (refund.Status == PaymentStatus.Completed)
            {
                refund.Order!.Status = OrderStatus.Refunded;
                await _db.SaveChangesAsync();
            }

            return refund.ToDto();
        }

        // -----------------------------
        // DELETE REFUND
        // -----------------------------
        public async Task<bool> DeleteRefundAsync(long id)
        {
            var refund = await _db.Refunds.FindAsync(id);
            if (refund == null) return false;

            _db.Refunds.Remove(refund);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
