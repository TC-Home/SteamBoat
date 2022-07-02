using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class bidchecks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BidCheck1Pass",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BidCheck1Score",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BidCheck2Pass",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BidCheck2Score",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BidCheck3Pass",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BidCheck3Score",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BidCheck1Pass",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "BidCheck1Score",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "BidCheck2Pass",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "BidCheck2Score",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "BidCheck3Pass",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "BidCheck3Score",
                table: "Items");
        }
    }
}
