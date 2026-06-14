using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eVote360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LeaderAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PoliticalLeaderAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PoliticalPartyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticalLeaderAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoliticalLeaderAssignments_PoliticalParties_PoliticalPartyId",
                        column: x => x.PoliticalPartyId,
                        principalTable: "PoliticalParties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoliticalLeaderAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PoliticalLeaderAssignments_PoliticalPartyId",
                table: "PoliticalLeaderAssignments",
                column: "PoliticalPartyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticalLeaderAssignments_UserId",
                table: "PoliticalLeaderAssignments",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoliticalLeaderAssignments");
        }
    }
}
