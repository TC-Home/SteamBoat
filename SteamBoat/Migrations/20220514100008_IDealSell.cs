using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class IDealSell : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "autoBidint",
                table: "Items",
                newName: "IdealSellInt");

            migrationBuilder.RenameColumn(
                name: "autoBidStr",
                table: "Items",
                newName: "IdealSell_Notes");

            migrationBuilder.RenameColumn(
                name: "autoBidNotes",
                table: "Items",
                newName: "IdealSellStr");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdealSell_Notes",
                table: "Items",
                newName: "autoBidStr");

            migrationBuilder.RenameColumn(
                name: "IdealSellStr",
                table: "Items",
                newName: "autoBidNotes");

            migrationBuilder.RenameColumn(
                name: "IdealSellInt",
                table: "Items",
                newName: "autoBidint");
        }
    }
}
