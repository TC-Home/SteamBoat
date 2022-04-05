using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class sales : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemsForSale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Game_hash_name_key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sale_price_after_fees = table.Column<int>(type: "int", nullable: false),
                    Itemhash_name_key = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsForSale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsForSale_Items_Itemhash_name_key",
                        column: x => x.Itemhash_name_key,
                        principalTable: "Items",
                        principalColumn: "hash_name_key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemsForSale_Itemhash_name_key",
                table: "ItemsForSale",
                column: "Itemhash_name_key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemsForSale");
        }
    }
}
