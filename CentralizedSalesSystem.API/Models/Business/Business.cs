using System;
using System.ComponentModel.DataAnnotations;
using CentralizedSalesSystem.API.Models.Auth;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Reservations;

namespace CentralizedSalesSystem.API.Models.Business;

public class Business
{
    public long Id { get; set; }

    [MaxLength(200)]
    [Required]
    public required string Name { get; set; }

    [MaxLength(50)]
    [Required]
    public required string Phone { get; set; }

    [MaxLength(400)]
    [Required]
    public required string Address { get; set; }

    [MaxLength(320)]
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [MaxLength(100)]
    [Required]
    public required string Country { get; set; }

    public long? OwnerId { get; set; }
    public User? Owner { get; set; }

    [Required]
    public Currency Currency { get; set; }

    [Required]
    public SubscriptionPlan SubscriptionPlan { get; set; }

    public DateTime? NextPaymentDueDate { get; set; }

    [MaxLength(200)]
    public string? WorkingHours { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    public ICollection<Tax> Taxes { get; set; } = new List<Tax>();
    public ICollection<Table> Tables { get; set; } = new List<Table>();
    public ICollection<ServiceCharge> ServiceCharges { get; set; } = new List<ServiceCharge>();
    public ICollection<GiftCard> GiftCards { get; set; } = new List<GiftCard>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
