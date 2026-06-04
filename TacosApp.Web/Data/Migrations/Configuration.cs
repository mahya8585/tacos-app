using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TacosApp.Web.Models.Domain;

namespace TacosApp.Web.Data.Migrations
{
    /// <summary>
    /// EF Core マイグレーションツール用ファクトリ（dotnet ef コマンドから呼ばれる）
    /// </summary>
    public class TacosDbContextFactory : IDesignTimeDbContextFactory<TacosDbContext>
    {
        public TacosDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TacosDbContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("TacosDb"));

            return new TacosDbContext(optionsBuilder.Options);
        }
    }

    /// <summary>
    /// EF Core シードデータ拡張メソッド（OnModelCreating から呼び出し）
    /// </summary>
    public static class TacosDbContextSeed
    {
        public static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Menu>().HasData(
                new Menu { MenuId = 1, Name = "クラシックビーフタコス", Description = "こだわりのスパイスで味付けしたビーフと新鮮野菜のタコス", Price = 350, ImageUrl = "/images/beef-taco.jpg", IsAvailable = true, DisplayOrder = 1 },
                new Menu { MenuId = 2, Name = "チキンタコス", Description = "やわらかグリルチキンとアボカドソースのタコス", Price = 320, ImageUrl = "/images/chicken-taco.jpg", IsAvailable = true, DisplayOrder = 2 },
                new Menu { MenuId = 3, Name = "シュリンプタコス", Description = "プリプリのエビとマンゴーサルサのタコス", Price = 380, ImageUrl = "/images/shrimp-taco.jpg", IsAvailable = true, DisplayOrder = 3 },
                new Menu { MenuId = 4, Name = "ベジタコス", Description = "彩り豊かな野菜とブラックビーンズのタコス", Price = 300, ImageUrl = "/images/veg-taco.jpg", IsAvailable = true, DisplayOrder = 4 },
                new Menu { MenuId = 5, Name = "チーズケサディーヤ", Description = "とろけるチーズをたっぷり挟んだ香ばしいサイドディッシュ", Price = 560, ImageUrl = "/images/quesadilla.jpg", IsAvailable = true, DisplayOrder = 5 },
                new Menu { MenuId = 6, Name = "メキシカンナチョス", Description = "クリスピーなチップスにサルサとチーズを重ねた人気サイド", Price = 260, ImageUrl = "/images/nachos.jpg", IsAvailable = true, DisplayOrder = 6 },
                new Menu { MenuId = 7, Name = "ライムソーダ", Description = "爽やかなライムの酸味が効いた炭酸ドリンク", Price = 270, ImageUrl = "/images/lime-soda.jpg", IsAvailable = true, DisplayOrder = 7 },
                new Menu { MenuId = 8, Name = "マンゴーラッシー", Description = "濃厚なマンゴーの甘みとヨーグルトのまろやかさが楽しいドリンク", Price = 440, ImageUrl = "/images/mango-lassi.jpg", IsAvailable = true, DisplayOrder = 8 }
            );

            modelBuilder.Entity<Topping>().HasData(
                new Topping { ToppingId = 1, Name = "グアカモーレ", Price = 80, IsAvailable = true, DisplayOrder = 1 },
                new Topping { ToppingId = 2, Name = "追加チーズ", Price = 50, IsAvailable = true, DisplayOrder = 2 },
                new Topping { ToppingId = 3, Name = "ハラペーニョ", Price = 30, IsAvailable = true, DisplayOrder = 3 },
                new Topping { ToppingId = 4, Name = "サワークリーム", Price = 50, IsAvailable = true, DisplayOrder = 4 },
                new Topping { ToppingId = 5, Name = "サルサ", Price = 30, IsAvailable = true, DisplayOrder = 5 },
                new Topping { ToppingId = 6, Name = "パクチー", Price = 20, IsAvailable = true, DisplayOrder = 6 }
            );
        }
    }
}