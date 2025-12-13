using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Mappers;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.PaymentDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly CentralizedSalesDbContext _db;
        private readonly IOrderService _orderService;


        public PaymentService(CentralizedSalesDbContext db, IOrderService orderService)
        {
            _db = db;
            _orderService = orderService;
        }

        public async Task<object> GetPaymentsAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = null,
            PaymentMethod? filterByMethod = null,
            DateTimeOffset? filterByCreatedAt = null,
            PaymentCurrency? filterByCurrency = null,
            PaymentStatus? filterByStatus = null,
            long? filterByOrderId = null)
        {
            var query = _db.Payments.AsQueryable();

            // Filters
            if (filterByMethod.HasValue)
                query = query.Where(x => x.Method == filterByMethod.Value);

            if (filterByCreatedAt.HasValue)
                query = query.Where(x => x.PaidAt.Date == filterByCreatedAt.Value.Date);

            if (filterByCurrency.HasValue)
                query = query.Where(x => x.Currency == filterByCurrency.Value);

            if (filterByStatus.HasValue)
                query = query.Where(x => x.Status == filterByStatus.Value);

            if (filterByOrderId.HasValue)
                query = query.Where(x => x.OrderId == filterByOrderId.Value);

            // Sorting
            bool asc = (sortDirection?.ToLower() ?? "asc") != "desc";

            query = sortBy?.ToLower() switch
            {
                "amount" => asc ? query.OrderBy(x => x.Amount) : query.OrderByDescending(x => x.Amount),
                "method" => asc ? query.OrderBy(x => x.Method) : query.OrderByDescending(x => x.Method),
                "paidat" => asc ? query.OrderBy(x => x.PaidAt) : query.OrderByDescending(x => x.PaidAt),
                "currency" => asc ? query.OrderBy(x => x.Currency) : query.OrderByDescending(x => x.Currency),
                "status" => asc ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
                _ => query.OrderBy(x => x.Id)
            };

            // Pagination
            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(total / (double)limit);

            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return new
            {
                data = items.Select(x => x.ToDto()),
                page,
                limit,
                total,
                totalPages
            };
        }

        public async Task<PaymentResponseDto?> GetPaymentByIdAsync(long id)
        {
            var payment = await _db.Payments.FindAsync(id);
            return payment?.ToDto();
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(PaymentCreateDto dto)
        {
            var order = await _db.Orders
                .Include(o => o.Discount)
                .Include(o => o.Items).ThenInclude(i => i.Item)
                .Include(o => o.Items).ThenInclude(i => i.Discount)
                .Include(o => o.Items).ThenInclude(i => i.Tax)
                .Include(o => o.Items).ThenInclude(i => i.ServiceCharge)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                throw new Exception($"Order {dto.OrderId} not found");

            var orderDto = order.ToOrderResponse();
            orderDto.Items = order.Items.Select(i => i.ToDto()).ToList();
            _orderService.CalculateOrderTotals(order, orderDto);

            var payment = new Payment
            {
                Amount = dto.Amount,
                PaidAt = DateTimeOffset.UtcNow,
                Method = dto.Method,
                Provider = dto.Provider,
                Currency = dto.Currency,
                Status = dto.Status,
                OrderId = dto.OrderId,
                BussinesId = dto.BussinesId
            };

            // Attach GiftCard if provided
            if (dto.GiftCardId.HasValue)
            {
                var giftCard = await _db.GiftCards.FindAsync(dto.GiftCardId.Value);
                if (giftCard == null)
                    throw new Exception("Gift card not found");

                payment.GiftCardId = giftCard.Id;
                payment.GiftCard = giftCard;
            }

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // If payment is completed, deduct gift card balance and recalc order
            if (payment.Status == PaymentStatus.Completed && payment.GiftCardId.HasValue)
            {
                var giftCard = payment.GiftCard!;
                if (giftCard.Status != GiftCardStatus.Valid)
                    throw new InvalidOperationException("Gift card is not valid");

                if (giftCard.CurrentBalance < payment.Amount)
                    throw new InvalidOperationException("Insufficient gift card balance");

                giftCard.CurrentBalance -= payment.Amount;
                if (giftCard.CurrentBalance == 0)
                    giftCard.Status = GiftCardStatus.Redeemed;

                await _db.SaveChangesAsync();
            }

            return payment.ToDto();
        }



        public async Task<PaymentResponseDto?> UpdatePaymentAsync(long id, PaymentUpdateDto dto)
        {
            var payment = await _db.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Items)
                .Include(p => p.Order)
                    .ThenInclude(o => o.Discount)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return null;

            // Check if status is changing to Completed BEFORE updating it
            bool completedNow =
                dto.Status.HasValue &&
                dto.Status.Value == PaymentStatus.Completed &&
                payment.Status != PaymentStatus.Completed;

            // Update mutable fields
            if (dto.Amount.HasValue)
                payment.Amount = dto.Amount.Value;

            if (dto.PaidAt.HasValue)
                payment.PaidAt = dto.PaidAt.Value;

            if (dto.Method.HasValue)
                payment.Method = dto.Method.Value;

            if (dto.Provider.HasValue)
                payment.Provider = dto.Provider.Value;

            if (dto.Currency.HasValue)
                payment.Currency = dto.Currency.Value;

            if (dto.Status.HasValue)
                payment.Status = dto.Status.Value;

            await _db.SaveChangesAsync();

            // Apply effects if the payment just became Completed
            if (completedNow)
            {
                // Deduct from gift card if applicable
                if (payment.GiftCardId.HasValue)
                {
                    var giftCard = await _db.GiftCards.FindAsync(payment.GiftCardId.Value);
                    if (giftCard == null)
                        throw new InvalidOperationException("Gift card not found");

                    if (giftCard.Status != GiftCardStatus.Valid)
                        throw new InvalidOperationException("Gift card is not valid");

                    if (giftCard.CurrentBalance < payment.Amount)
                        throw new InvalidOperationException("Insufficient gift card balance");

                    giftCard.CurrentBalance -= payment.Amount;

                    if (giftCard.CurrentBalance == 0)
                        giftCard.Status = GiftCardStatus.Redeemed;

                    await _db.SaveChangesAsync(); // Save gift card changes
                }

                // Recalculate order totals and remaining amount
                var order = payment.Order;
                var orderDto = order.ToOrderResponse();
                orderDto.Items = order.Items.Select(i => i.ToDto()).ToList();

                _orderService.CalculateOrderTotals(order, orderDto);

                decimal amountPaid = order.Payments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount);

                decimal remaining = orderDto.Total - amountPaid;

                if (remaining <= 0 && order.Status != OrderStatus.Closed)
                {
                    order.Status = OrderStatus.Closed;
                    await _db.SaveChangesAsync();
                }
            }

            return payment.ToDto();
        }



        public async Task<bool> DeletePaymentAsync(long id)
        {
            var payment = await _db.Payments.FindAsync(id);
            if (payment == null)
                return false;

            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
