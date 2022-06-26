using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class allowshark : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "allowshark",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "allowshark",
                table: "Items");
        }
    }
}
