using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TacosApp.Admin.Models;

namespace TacosApp.Admin.Data;

public class AdminDbContext(DbContextOptions<AdminDbContext> options)
    : IdentityDbContext<AdminUser>(options)
{
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<Topping> Toppings => Set<Topping>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemTopping> OrderItemToppings => Set<OrderItemTopping>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Menu>(e =>
        {
            e.ToTable("Menus");
            e.HasKey(x => x.MenuId);
        });

        builder.Entity<Topping>(e =>
        {
            e.ToTable("Toppings");
            e.HasKey(x => x.ToppingId);
        });

        builder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(x => x.OrderId);
            e.HasIndex(x => x.OrderNumber).IsUnique();
        });

        builder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasKey(x => x.OrderItemId);

            e.HasOne(x => x.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Menu)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(x => x.MenuId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderItemTopping>(e =>
        {
            e.ToTable("OrderItemToppings");
            e.HasKey(x => x.OrderItemToppingId);

            e.HasOne(x => x.OrderItem)
                .WithMany(oi => oi.Toppings)
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Topping)
                .WithMany(t => t.OrderItemToppings)
                .HasForeignKey(x => x.ToppingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
