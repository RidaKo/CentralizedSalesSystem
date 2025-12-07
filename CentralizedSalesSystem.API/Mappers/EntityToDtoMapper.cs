using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Auth;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.DTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TableDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Controllers.Orders;


namespace CentralizedSalesSystem.API.Mappers
{
    public static class EntityToDtoMapper
    {
        public static ReservationResponseDto ToDto(this CentralizedSalesSystem.API.Models.Reservation r)
        {
            return new ReservationResponseDto
            {
                Id = r.Id,
                BusinessId = r.BusinessId,
                CustomerName = r.CustomerName,
                CustomerPhone = r.CustomerPhone,
                CustomerNote = r.CustomerNote,
                AppointmentTime = r.AppointmentTime,
                CreatedAt = r.CreatedAt,
                CreatedBy = r.CreatedBy,
                Status = r.Status.ToString().ToLowerInvariant(),
                Items = r.Items?.Select(i => i.ToDto()).ToList(),
                AssignedEmployee = r.AssignedEmployee,
                GuestNumber = r.GuestNumber,
                TableId = r.TableId
            };
        }

        public static TableResponseDto ToDto(this Table t)
        {
            return new TableResponseDto
            {
                Id = t.Id,
                BusinessId = t.BusinessId,
                Name = t.Name,
                Capacity = t.Capacity,
                Status = t.Status
            };
        }
        public static OrderItemResponseDto ToDto(this OrderItem oi)
        {
            return new OrderItemResponseDto
            {
                Id = oi.Id,
                Quantity = oi.Quantity,
                Notes = oi.Notes,
                ItemId = oi.ItemId,
                DiscountId = oi.DiscountId,
                OrderId = oi.OrderId,
                Item = oi.Item != null ? new ItemResponseDto
                {
                    Id = oi.Item.Id,
                    Name = oi.Item.Name,
                    Description = oi.Item.Description,
                    Price = oi.Item.Price,
                    Type = oi.Item.Type,
                    Stock = oi.Item.Stock,
                    BusinessId = oi.Item.BusinessId,
                    Variations = oi.Item.Variations?.Select(v => new ItemVariationResponseDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                        ItemId = v.ItemId,
                        Selection = v.Selection
                    }).ToList() ?? new List<ItemVariationResponseDto>()
                } : null,
                Discount = oi.Discount != null ? new DiscountResponseDto
                {
                    Id = oi.Discount.Id,
                    Name = oi.Discount.Name,
                    Rate = oi.Discount.rate,
                    ValidFrom = oi.Discount.ValidFrom,
                    ValidTo = oi.Discount.ValidTo,
                    Type = oi.Discount.Type,
                    AppliesTo = oi.Discount.AppliesTo,
                    Status = oi.Discount.Status,
                    BusinessId = oi.Discount.BusinessId
                } : null,
                Taxes = oi.Taxes?.Select(t => new TaxResponseDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Rate = t.Rate,
                    BusinessId = t.BusinessId
                }).ToList() ?? new List<TaxResponseDto>(),
                ServiceCharges = oi.ServiceCharges?.Select(sc => new ServiceChargeResponseDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Rate = sc.rate,
                    BusinessId = sc.BusinessId
                }).ToList() ?? new List<ServiceChargeResponseDto>()
            };
        }
        public static OrderResponseDto ToOrderResponse(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                BusinessId = order.BusinessId,
                Tip = order.Tip,
                UpdatedAt = order.UpdatedAt,
                Status = order.Status,
                UserId = order.UserId,
                TableId = order.TableId,
                DiscountId = order.DiscountId,
                ReservationId = order.ReservationId
            };
        }
        public static ItemResponseDto ToDto(this Item item)
        {
            return new ItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
                Type = item.Type,
                BusinessId = item.BusinessId,
                Variations = item.Variations?.Select(v => new ItemVariationResponseDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    ItemId = v.ItemId,
                    Selection = v.Selection
                }).ToList(),
                AssociatedRoles = item.Type == Models.Orders.enums.ItemType.service && item.AssociatedRoles != null
                    ? item.AssociatedRoles.ToList()
                    : null
            };
        }
        public static ItemVariationResponseDto ToDto(this ItemVariation variation)
        {
            return new ItemVariationResponseDto
            {
                Id = variation.Id,
                Name = variation.Name,
                ItemId = variation.ItemId,
                Selection = variation.Selection
            };
        }
        public static TaxResponseDto ToDto(this Tax tax)
        {
            return new TaxResponseDto
            {
                Id = tax.Id,
                Name = tax.Name,
                Rate = tax.Rate,
                CreatedAt = tax.CreatedAt,
                EffectiveFrom = tax.EffectiveFrom,
                EffectiveTo = tax.EffectiveTo,
                Status = tax.Status,
                BusinessId = tax.BusinessId
            };
        }
        public static DiscountResponseDto ToDto(this Discount discount)
        {
            return new DiscountResponseDto
            {
                Id = discount.Id,
                Name = discount.Name,
                Rate = discount.rate,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo,
                Type = discount.Type,
                AppliesTo = discount.AppliesTo,
                Status = discount.Status,
                BusinessId = discount.BusinessId
            };
        }
        public static ServiceChargeResponseDto ToDto(this ServiceCharge sc)
        {
            return new ServiceChargeResponseDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Rate = sc.rate,
                CreatedAt = sc.CreatedAt,
                UpdatedAt = sc.UpdatedAt,
                BusinessId = sc.BusinessId
            };
        }

    }
}
