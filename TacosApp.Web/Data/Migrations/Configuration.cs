using System.Data.Entity.Migrations;
using TacosApp.Web.Models.Domain;

namespace TacosApp.Web.Data.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<TacosDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "TacosApp.Web.Data.TacosDbContext";
        }

        protected override void Seed(TacosDbContext context)
        {
            // メニューのシードデータ
            context.Menus.AddOrUpdate(m => m.MenuId,
                new Menu
                {
                    MenuId = 1,
                    Name = "クラシックビーフタコス",
                    Description = "こだわりのスパイスで味付けしたビーフと新鮮野菜のタコス",
                    Price = 350,
                    ImageUrl = "/Content/images/beef-taco.jpg",
                    IsAvailable = true,
                    DisplayOrder = 1
                },
                new Menu
                {
                    MenuId = 2,
                    Name = "チキンタコス",
                    Description = "やわらかグリルチキンとアボカドソースのタコス",
                    Price = 320,
                    ImageUrl = "/Content/images/chicken-taco.jpg",
                    IsAvailable = true,
                    DisplayOrder = 2
                },
                new Menu
                {
                    MenuId = 3,
                    Name = "シュリンプタコス",
                    Description = "プリプリのエビとマンゴーサルサのタコス",
                    Price = 380,
                    ImageUrl = "/Content/images/shrimp-taco.jpg",
                    IsAvailable = true,
                    DisplayOrder = 3
                },
                new Menu
                {
                    MenuId = 4,
                    Name = "ベジタコス",
                    Description = "彩り豊かな野菜とブラックビーンズのタコス",
                    Price = 300,
                    ImageUrl = "/Content/images/veg-taco.jpg",
                    IsAvailable = true,
                    DisplayOrder = 4
                }
            );

            // トッピングのシードデータ
            context.Toppings.AddOrUpdate(t => t.ToppingId,
                new Topping { ToppingId = 1, Name = "グアカモーレ", Price = 80, IsAvailable = true, DisplayOrder = 1 },
                new Topping { ToppingId = 2, Name = "追加チーズ", Price = 50, IsAvailable = true, DisplayOrder = 2 },
                new Topping { ToppingId = 3, Name = "ハラペーニョ", Price = 30, IsAvailable = true, DisplayOrder = 3 },
                new Topping { ToppingId = 4, Name = "サワークリーム", Price = 50, IsAvailable = true, DisplayOrder = 4 },
                new Topping { ToppingId = 5, Name = "サルサ", Price = 30, IsAvailable = true, DisplayOrder = 5 },
                new Topping { ToppingId = 6, Name = "パクチー", Price = 20, IsAvailable = true, DisplayOrder = 6 }
            );

            context.SaveChanges();
        }
    }
}
