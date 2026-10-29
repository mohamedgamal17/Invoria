using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Financial.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddReceivable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Receivables",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PartyId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receivables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceivableSettlements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReceivableId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SettledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivableSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivableSettlements_Receivables_ReceivableId",
                        column: x => x.ReceivableId,
                        principalTable: "Receivables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_CreatedAt",
                table: "Receivables",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_CreatedBy",
                table: "Receivables",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_LastModifiedAt",
                table: "Receivables",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_LastModifiedBy",
                table: "Receivables",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_PartyId",
                table: "Receivables",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_SourceId",
                table: "Receivables",
                column: "SourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceivableSettlements_CreatedAt",
                table: "ReceivableSettlements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivableSettlements_CreatedBy",
                table: "ReceivableSettlements",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivableSettlements_LastModifiedAt",
                table: "ReceivableSettlements",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivableSettlements_LastModifiedBy",
                table: "ReceivableSettlements",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivableSettlements_ReceivableId",
                table: "ReceivableSettlements",
                column: "ReceivableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceivableSettlements");

            migrationBuilder.DropTable(
                name: "Receivables");
        }
    }
}
