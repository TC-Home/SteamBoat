using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class predictor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Pred_Tip_Price",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tip_Price1",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tip_Price10",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tip_Price2",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tip_Price3",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tip_Price4",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tip_Price5",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tip_Price6",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tip_Price7",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tip_Price8",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tip_Price9",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pred_Tip_Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Tip_Price1",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Tip_Price10",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Tip_Price2",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Tip_Price3",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Tip_Price4",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Tip_Price5",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Tip_Price6",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Tip_Price7",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Tip_Price8",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Tip_Price9",
                table: "Items");
        }
    }
}
