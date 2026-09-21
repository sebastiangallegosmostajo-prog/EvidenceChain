using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvidenceChain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustodyEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustodyEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvidenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromCustodianId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToCustodianId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PreviousHash = table.Column<string>(type: "char(64)", nullable: false),
                    Hash = table.Column<string>(type: "char(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustodyEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustodyEvents_Evidence_EvidenceId",
                        column: x => x.EvidenceId,
                        principalTable: "Evidence",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustodyEvents_Users_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustodyEvents_Users_FromCustodianId",
                        column: x => x.FromCustodianId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustodyEvents_Users_ToCustodianId",
                        column: x => x.ToCustodianId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustodyEvents_ActorId",
                table: "CustodyEvents",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_CustodyEvents_EvidenceId_OccurredAtUtc",
                table: "CustodyEvents",
                columns: new[] { "EvidenceId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CustodyEvents_EvidenceId_SequenceNumber",
                table: "CustodyEvents",
                columns: new[] { "EvidenceId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustodyEvents_FromCustodianId",
                table: "CustodyEvents",
                column: "FromCustodianId");

            migrationBuilder.CreateIndex(
                name: "IX_CustodyEvents_ToCustodianId",
                table: "CustodyEvents",
                column: "ToCustodianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustodyEvents");
        }
    }
}
