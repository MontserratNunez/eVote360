using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eVote360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixCandidateAssign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssignPositions_CandidateId_ElectivePositionId_PoliticalPartyId",
                table: "AssignPositions");

            migrationBuilder.DropIndex(
                name: "IX_AssignPositions_ElectionId",
                table: "AssignPositions");

            migrationBuilder.CreateIndex(
                name: "IX_AssignPositions_CandidateId",
                table: "AssignPositions",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignPositions_ElectionId_CandidateId_ElectivePositionId_PoliticalPartyId",
                table: "AssignPositions",
                columns: new[] { "ElectionId", "CandidateId", "ElectivePositionId", "PoliticalPartyId" },
                unique: true,
                filter: "[ElectionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssignPositions_CandidateId",
                table: "AssignPositions");

            migrationBuilder.DropIndex(
                name: "IX_AssignPositions_ElectionId_CandidateId_ElectivePositionId_PoliticalPartyId",
                table: "AssignPositions");

            migrationBuilder.CreateIndex(
                name: "IX_AssignPositions_CandidateId_ElectivePositionId_PoliticalPartyId",
                table: "AssignPositions",
                columns: new[] { "CandidateId", "ElectivePositionId", "PoliticalPartyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignPositions_ElectionId",
                table: "AssignPositions",
                column: "ElectionId");
        }
    }
}
