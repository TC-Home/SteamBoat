using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class shark_max : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SharkMaxPrice",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharkMaxPrice",
                table: "Items");
        }
    }
}
