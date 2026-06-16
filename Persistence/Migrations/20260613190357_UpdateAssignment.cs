using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eVote360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ElectionId",
                table: "AssignPositions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignPositions_ElectionId",
                table: "AssignPositions",
                column: "ElectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignPositions_Elections_ElectionId",
                table: "AssignPositions",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignPositions_Elections_ElectionId",
                table: "AssignPositions");

            migrationBuilder.DropIndex(
                name: "IX_AssignPositions_ElectionId",
                table: "AssignPositions");

            migrationBuilder.DropColumn(
                name: "ElectionId",
                table: "AssignPositions");
        }
    }
}
