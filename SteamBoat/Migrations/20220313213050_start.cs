using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamBoat.Migrations
{
    public partial class start : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mission",
                columns: table => new
                {
                    MissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ListingsMin = table.Column<int>(type: "int", nullable: false),
                    ListingsMax = table.Column<int>(type: "int", nullable: false),
                    PriceMin = table.Column<int>(type: "int", nullable: false),
                    PriceMax = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mission", x => x.MissionId);
                });

            migrationBuilder.CreateTable(
                name: "FeederUrl",
                columns: table => new
                {
                    FeederId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissionId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    isJSON = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeederUrl", x => x.FeederId);
                    table.ForeignKey(
                        name: "FK_FeederUrl_Mission_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Mission",
                        principalColumn: "MissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeederUrl_MissionId",
                table: "FeederUrl",
                column: "MissionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeederUrl");

            migrationBuilder.DropTable(
                name: "Mission");
        }
    }
}
