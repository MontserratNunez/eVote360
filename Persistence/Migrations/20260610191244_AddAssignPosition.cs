using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eVote360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    ElectivePositionId = table.Column<int>(type: "int", nullable: false),
                    PoliticalPartyId = table.Column<int>(type: "int", nullable: false),
                    IsAlliance = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignPositions_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignPositions_ElectivePositions_ElectivePositionId",
                        column: x => x.ElectivePositionId,
                        principalTable: "ElectivePositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignPositions_PoliticalParties_PoliticalPartyId",
                        column: x => x.PoliticalPartyId,
                        principalTable: "PoliticalParties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignPositions_CandidateId_ElectivePositionId_PoliticalPartyId",
                table: "AssignPositions",
                columns: new[] { "CandidateId", "ElectivePositionId", "PoliticalPartyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignPositions_ElectivePositionId",
                table: "AssignPositions",
                column: "ElectivePositionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignPositions_PoliticalPartyId",
                table: "AssignPositions",
                column: "PoliticalPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignPositions");
        }
    }
}
