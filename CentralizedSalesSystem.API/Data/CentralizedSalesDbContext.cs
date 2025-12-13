using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Auth;
using CentralizedSalesSystem.API.Models.Business;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Reservations;


using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Data
{
    public class CentralizedSalesDbContext: DbContext
    {   
        //Auth
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; } 
        public DbSet<RolePermission> RolePermissions { get; set; }

        //Business
        public DbSet<Business> Businesses { get; set; }

        //Orders
        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ServiceCharge> ServiceCharges { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<ItemVariation> ItemVariations { get; set; }
        public DbSet<ItemVariationOption> ItemVariationOptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<GiftCard> GiftCards { get; set; }






        // Reservations
        public DbSet<Reservation> Reservations { get; set; }






        public CentralizedSalesDbContext(DbContextOptions<CentralizedSalesDbContext> options) : base(options) { 
                
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Avoid shadow FK for Business.OwnerId
            modelBuilder.Entity<Business>()
                .HasOne(b => b.Owner)
                .WithMany() // Owner navigation is independent from Users collection
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Break multiple cascade path for Refunds (Order -> Refund and Order -> Payment -> Refund)
            modelBuilder.Entity<Refund>()
                .HasOne(r => r.Payment)
                .WithMany()
                .HasForeignKey(r => r.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Refund>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Break cascade path Business -> Orders when Business also cascades via Users
            modelBuilder.Entity<Order>()
                .HasOne<Business>()
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            // Break cascade path Business -> Users (and downstream chains)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Business)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.GiftCard)
                .WithMany(g => g.Redemptions)
                .HasForeignKey(p => p.GiftCardId)
                .OnDelete(DeleteBehavior.Restrict);


            // Decimal precision
            modelBuilder.Entity<Item>().Property(i => i.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ItemVariationOption>().Property(o => o.PriceAdjustment).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ServiceCharge>().Property(s => s.rate).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Tax>().Property(t => t.Rate).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Discount>().Property(d => d.rate).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>().Property(o => o.Tip).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Refund>().Property(r => r.Amount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<GiftCard>().Property(g => g.InitialValue).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<GiftCard>().Property(g => g.CurrentBalance).HasColumnType("decimal(18,2)");
        }

    }
}
