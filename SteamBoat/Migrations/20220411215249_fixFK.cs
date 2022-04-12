using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class fixFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsForSale_Items_Itemhash_name_key",
                table: "ItemsForSale");

            migrationBuilder.DropIndex(
                name: "IX_ItemsForSale_Itemhash_name_key",
                table: "ItemsForSale");

            migrationBuilder.DropColumn(
                name: "Itemhash_name_key",
                table: "ItemsForSale");

            migrationBuilder.AlterColumn<string>(
                name: "Game_hash_name_key",
                table: "ItemsForSale",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemsForSale_Game_hash_name_key",
                table: "ItemsForSale",
                column: "Game_hash_name_key");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsForSale_Items_Game_hash_name_key",
                table: "ItemsForSale",
                column: "Game_hash_name_key",
                principalTable: "Items",
                principalColumn: "hash_name_key",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsForSale_Items_Game_hash_name_key",
                table: "ItemsForSale");

            migrationBuilder.DropIndex(
                name: "IX_ItemsForSale_Game_hash_name_key",
                table: "ItemsForSale");

            migrationBuilder.AlterColumn<string>(
                name: "Game_hash_name_key",
                table: "ItemsForSale",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Itemhash_name_key",
                table: "ItemsForSale",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemsForSale_Itemhash_name_key",
                table: "ItemsForSale",
                column: "Itemhash_name_key");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsForSale_Items_Itemhash_name_key",
                table: "ItemsForSale",
                column: "Itemhash_name_key",
                principalTable: "Items",
                principalColumn: "hash_name_key",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
