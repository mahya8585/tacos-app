using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TacosApp.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,0)", precision: 10, scale: 0, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.MenuId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DeliveryNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,0)", precision: 10, scale: 0, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OrderedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "Toppings",
                columns: table => new
                {
                    ToppingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,0)", precision: 10, scale: 0, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Toppings", x => x.ToppingId);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,0)", precision: 10, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemToppings",
                columns: table => new
                {
                    OrderItemToppingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    ToppingId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,0)", precision: 10, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemToppings", x => x.OrderItemToppingId);
                    table.ForeignKey(
                        name: "FK_OrderItemToppings_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "OrderItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItemToppings_Toppings_ToppingId",
                        column: x => x.ToppingId,
                        principalTable: "Toppings",
                        principalColumn: "ToppingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "MenuId", "Description", "DisplayOrder", "ImageUrl", "IsAvailable", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "こだわりのスパイスで味付けしたビーフと新鮮野菜のタコス", 1, "/images/beef-taco.jpg", true, "クラシックビーフタコス", 350m },
                    { 2, "やわらかグリルチキンとアボカドソースのタコス", 2, "/images/chicken-taco.jpg", true, "チキンタコス", 320m },
                    { 3, "プリプリのエビとマンゴーサルサのタコス", 3, "/images/shrimp-taco.jpg", true, "シュリンプタコス", 380m },
                    { 4, "彩り豊かな野菜とブラックビーンズのタコス", 4, "/images/veg-taco.jpg", true, "ベジタコス", 300m },
                    { 5, "とろけるチーズをたっぷり挟んだ香ばしいサイドディッシュ", 5, "/images/quesadilla.jpg", true, "チーズケサディーヤ", 560m },
                    { 6, "クリスピーなチップスにサルサとチーズを重ねた人気サイド", 6, "/images/nachos.jpg", true, "メキシカンナチョス", 260m },
                    { 7, "爽やかなライムの酸味が効いた炭酸ドリンク", 7, "/images/lime-soda.jpg", true, "ライムソーダ", 270m },
                    { 8, "濃厚なマンゴーの甘みとヨーグルトのまろやかさが楽しいドリンク", 8, "/images/mango-lassi.jpg", true, "マンゴーラッシー", 440m }
                });

            migrationBuilder.InsertData(
                table: "Toppings",
                columns: new[] { "ToppingId", "DisplayOrder", "IsAvailable", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 1, true, "グアカモーレ", 80m },
                    { 2, 2, true, "追加チーズ", 50m },
                    { 3, 3, true, "ハラペーニョ", 30m },
                    { 4, 4, true, "サワークリーム", 50m },
                    { 5, 5, true, "サルサ", 30m },
                    { 6, 6, true, "パクチー", 20m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuId",
                table: "OrderItems",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemToppings_OrderItemId",
                table: "OrderItemToppings",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemToppings_ToppingId",
                table: "OrderItemToppings",
                column: "ToppingId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemToppings");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Toppings");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
