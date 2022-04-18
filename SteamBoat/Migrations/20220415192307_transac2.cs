using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class transac2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Tran_Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Game_hash_name_key = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    int_sale_price_after_fees = table.Column<int>(type: "int", nullable: false),
                    sale_price_after_fees = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateT = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Tran_Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Items_Game_hash_name_key",
                        column: x => x.Game_hash_name_key,
                        principalTable: "Items",
                        principalColumn: "hash_name_key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Game_hash_name_key",
                table: "Transaction",
                column: "Game_hash_name_key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transaction");
        }
    }
}
