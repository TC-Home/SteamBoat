using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class multiplesalecount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastNumberSold",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastNumberSold",
                table: "Items");
        }
    }
}
