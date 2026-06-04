using Microsoft.EntityFrameworkCore;
using TacosApp.Web.Models.Domain;

namespace TacosApp.Web.Data
{
    public class TacosDbContext : DbContext
    {
        public TacosDbContext(DbContextOptions<TacosDbContext> options) : base(options)
        {
        }

        public DbSet<Menu> Menus { get; set; }
        public DbSet<Topping> Toppings { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemTopping> OrderItemToppings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // テーブル名を明示的に指定（複数形のまま）
            modelBuilder.Entity<Menu>().ToTable("Menus");
            modelBuilder.Entity<Topping>().ToTable("Toppings");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            modelBuilder.Entity<OrderItemTopping>().ToTable("OrderItemToppings");

            // Order.OrderNumber に Unique インデックス
            modelBuilder.Entity<Order>()
                .Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(30);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // decimal 精度の明示指定
            modelBuilder.Entity<Menu>()
                .Property(m => m.Price)
                .HasPrecision(10, 0);

            modelBuilder.Entity<Topping>()
                .Property(t => t.Price)
                .HasPrecision(10, 0);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(10, 0);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(10, 0);

            modelBuilder.Entity<OrderItemTopping>()
                .Property(oit => oit.UnitPrice)
                .HasPrecision(10, 0);

            // シードデータ
            TacosApp.Web.Data.Migrations.TacosDbContextSeed.SeedData(modelBuilder);
        }
    }
}
