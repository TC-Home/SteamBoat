using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class moreSaleItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "differnce",
                table: "ItemsForSale",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "max_buy_bid",
                table: "ItemsForSale",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sale_price",
                table: "ItemsForSale",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "differnce",
                table: "ItemsForSale");

            migrationBuilder.DropColumn(
                name: "max_buy_bid",
                table: "ItemsForSale");

            migrationBuilder.DropColumn(
                name: "sale_price",
                table: "ItemsForSale");
        }
    }
}
