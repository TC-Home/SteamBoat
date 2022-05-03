using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class hhf : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Fruit2",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sell1Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sell1Quant",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sell2Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sell2Quant",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sell3Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sell3Quant",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sell4Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sell4Quant",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sell5Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sell5Quant",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fruit2",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "sell1Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "sell1Quant",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "sell2Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "sell2Quant",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "sell3Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "sell3Quant",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "sell4Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "sell4Quant",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "sell5Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "sell5Quant",
                table: "Items");
        }
    }
}
