using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class newautobid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CancelCurrentBid",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IdealBidInt",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdealBidStr",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdealBid_Notes",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelCurrentBid",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "IdealBidInt",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "IdealBidStr",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "IdealBid_Notes",
                table: "Items");
        }
    }
}
