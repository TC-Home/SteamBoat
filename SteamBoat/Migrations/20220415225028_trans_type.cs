using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class trans_type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Items_Game_hash_name_key",
                table: "Transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction");

            migrationBuilder.RenameTable(
                name: "Transaction",
                newName: "Transactions");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_Game_hash_name_key",
                table: "Transactions",
                newName: "IX_Transactions_Game_hash_name_key");

            migrationBuilder.AlterColumn<string>(
                name: "Tran_Id",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "Transactions",
                type: "nvarchar(1)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "type");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Items_Game_hash_name_key",
                table: "Transactions",
                column: "Game_hash_name_key",
                principalTable: "Items",
                principalColumn: "hash_name_key",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Items_Game_hash_name_key",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "type",
                table: "Transactions");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Transaction");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_Game_hash_name_key",
                table: "Transaction",
                newName: "IX_Transaction_Game_hash_name_key");

            migrationBuilder.AlterColumn<string>(
                name: "Tran_Id",
                table: "Transaction",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction",
                column: "Tran_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Items_Game_hash_name_key",
                table: "Transaction",
                column: "Game_hash_name_key",
                principalTable: "Items",
                principalColumn: "hash_name_key",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
