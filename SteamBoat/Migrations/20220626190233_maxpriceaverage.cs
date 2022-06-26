using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class maxpriceaverage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tip_PriceAVG",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tip_PriceAVG",
                table: "Items");
        }
    }
}
