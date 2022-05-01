using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class _5bids : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bid1Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bid1Quant",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bid2Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bid2Quant",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bid3Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bid3Quant",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bid4Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bid4Quant",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bid5Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "bid5Quant",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bid1Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "bid1Quant",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "bid2Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "bid2Quant",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "bid3Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "bid3Quant",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "bid4Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "bid4Quant",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "bid5Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "bid5Quant",
                table: "Items");
        }
    }
}
