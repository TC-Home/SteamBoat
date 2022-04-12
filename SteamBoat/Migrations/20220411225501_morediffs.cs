using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class morediffs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "differnce",
                table: "ItemsForSale",
                newName: "sale_price_diff");

            migrationBuilder.AddColumn<int>(
                name: "max_buy_bid_diff",
                table: "ItemsForSale",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_buy_bid_diff",
                table: "ItemsForSale");

            migrationBuilder.RenameColumn(
                name: "sale_price_diff",
                table: "ItemsForSale",
                newName: "differnce");
        }
    }
}
