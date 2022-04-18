using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class stats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Ave_buy",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ave_profic_pc",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ave_profit",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ave_sell",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "total_buys",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "total_profit",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "total_profit_including_stock",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "total_sales",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ave_buy",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Ave_profic_pc",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Ave_profit",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Ave_sell",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "total_buys",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "total_profit",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "total_profit_including_stock",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "total_sales",
                table: "Items");
        }
    }
}
