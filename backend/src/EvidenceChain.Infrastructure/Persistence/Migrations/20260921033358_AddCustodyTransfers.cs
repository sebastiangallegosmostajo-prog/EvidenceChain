using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvidenceChain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustodyTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustodyTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvidenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromCustodianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToCustodianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RespondedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustodyTransfers", x => x.Id);
                    table.CheckConstraint("CK_CustodyTransfers_DifferentCustodians", "[FromCustodianId] <> [ToCustodianId]");
                    table.CheckConstraint("CK_CustodyTransfers_ValidExpiration", "[ExpiresAtUtc] > [RequestedAtUtc]");
                    table.ForeignKey(
                        name: "FK_CustodyTransfers_Evidence_EvidenceId",
                        column: x => x.EvidenceId,
                        principalTable: "Evidence",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustodyTransfers_Users_FromCustodianId",
                        column: x => x.FromCustodianId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustodyTransfers_Users_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustodyTransfers_Users_ToCustodianId",
                        column: x => x.ToCustodianId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustodyTransfers_EvidenceId_Status",
                table: "CustodyTransfers",
                columns: new[] { "EvidenceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CustodyTransfers_FromCustodianId",
                table: "CustodyTransfers",
                column: "FromCustodianId");

            migrationBuilder.CreateIndex(
                name: "IX_CustodyTransfers_RequestedById",
                table: "CustodyTransfers",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_CustodyTransfers_ToCustodianId_Status",
                table: "CustodyTransfers",
                columns: new[] { "ToCustodianId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustodyTransfers");
        }
    }
}
