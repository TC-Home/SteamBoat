using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class activityHis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AH1",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AH10",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AH2",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AH3",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AH4",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AH5",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AH6",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AH7",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AH8",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AH9",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ActivityHistory",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ItemPriceURL",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AH1",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AH10",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AH2",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AH3",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AH4",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AH5",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AH6",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AH7",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AH8",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AH9",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ActivityHistory",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ItemPriceURL",
                table: "Items");
        }
    }
}
