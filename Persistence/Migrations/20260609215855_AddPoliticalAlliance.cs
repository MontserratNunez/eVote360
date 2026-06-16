using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eVote360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPoliticalAlliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PoliticalAlliances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestingPartyId = table.Column<int>(type: "int", nullable: false),
                    ReceivingPartyId = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticalAlliances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoliticalAlliances_PoliticalParties_ReceivingPartyId",
                        column: x => x.ReceivingPartyId,
                        principalTable: "PoliticalParties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoliticalAlliances_PoliticalParties_RequestingPartyId",
                        column: x => x.RequestingPartyId,
                        principalTable: "PoliticalParties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PoliticalAlliances_ReceivingPartyId",
                table: "PoliticalAlliances",
                column: "ReceivingPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticalAlliances_RequestingPartyId_ReceivingPartyId",
                table: "PoliticalAlliances",
                columns: new[] { "RequestingPartyId", "ReceivingPartyId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoliticalAlliances");
        }
    }
}
