using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eVote360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPoliticalPartyToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PoliticalPartyId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    PoliticalPartyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidates_PoliticalParties_PoliticalPartyId",
                        column: x => x.PoliticalPartyId,
                        principalTable: "PoliticalParties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PoliticalAlliances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestingPoliticalPartyId = table.Column<int>(type: "int", nullable: false),
                    RequestedPoliticalPartyId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticalAlliances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoliticalAlliances_PoliticalParties_RequestedPoliticalPartyId",
                        column: x => x.RequestedPoliticalPartyId,
                        principalTable: "PoliticalParties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoliticalAlliances_PoliticalParties_RequestingPoliticalPartyId",
                        column: x => x.RequestingPoliticalPartyId,
                        principalTable: "PoliticalParties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CandidatePositionAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    ElectivePositionId = table.Column<int>(type: "int", nullable: false),
                    PoliticalPartyId = table.Column<int>(type: "int", nullable: false),
                    IsAlliedCandidate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidatePositionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidatePositionAssignments_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidatePositionAssignments_ElectivePositions_ElectivePositionId",
                        column: x => x.ElectivePositionId,
                        principalTable: "ElectivePositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidatePositionAssignments_PoliticalParties_PoliticalPartyId",
                        column: x => x.PoliticalPartyId,
                        principalTable: "PoliticalParties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PoliticalPartyId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PoliticalPartyId",
                table: "Users",
                column: "PoliticalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidatePositionAssignments_CandidateId",
                table: "CandidatePositionAssignments",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidatePositionAssignments_ElectivePositionId",
                table: "CandidatePositionAssignments",
                column: "ElectivePositionId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidatePositionAssignments_PoliticalPartyId_CandidateId",
                table: "CandidatePositionAssignments",
                columns: new[] { "PoliticalPartyId", "CandidateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidatePositionAssignments_PoliticalPartyId_ElectivePositionId",
                table: "CandidatePositionAssignments",
                columns: new[] { "PoliticalPartyId", "ElectivePositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_PoliticalPartyId",
                table: "Candidates",
                column: "PoliticalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticalAlliances_RequestedPoliticalPartyId",
                table: "PoliticalAlliances",
                column: "RequestedPoliticalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticalAlliances_RequestingPoliticalPartyId",
                table: "PoliticalAlliances",
                column: "RequestingPoliticalPartyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PoliticalParties_PoliticalPartyId",
                table: "Users",
                column: "PoliticalPartyId",
                principalTable: "PoliticalParties",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PoliticalParties_PoliticalPartyId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "CandidatePositionAssignments");

            migrationBuilder.DropTable(
                name: "PoliticalAlliances");

            migrationBuilder.DropTable(
                name: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Users_PoliticalPartyId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PoliticalPartyId",
                table: "Users");
        }
    }
}
