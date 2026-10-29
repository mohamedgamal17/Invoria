using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Financial.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddPayable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payables",
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
                    table.PrimaryKey("PK_Payables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayableSettlements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PayableId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SettledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayableSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayableSettlements_Payables_PayableId",
                        column: x => x.PayableId,
                        principalTable: "Payables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payables_CreatedAt",
                table: "Payables",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payables_CreatedBy",
                table: "Payables",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Payables_LastModifiedAt",
                table: "Payables",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payables_LastModifiedBy",
                table: "Payables",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Payables_PartyId",
                table: "Payables",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_Payables_SourceId",
                table: "Payables",
                column: "SourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayableSettlements_CreatedAt",
                table: "PayableSettlements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PayableSettlements_CreatedBy",
                table: "PayableSettlements",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PayableSettlements_LastModifiedAt",
                table: "PayableSettlements",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PayableSettlements_LastModifiedBy",
                table: "PayableSettlements",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PayableSettlements_PayableId",
                table: "PayableSettlements",
                column: "PayableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayableSettlements");

            migrationBuilder.DropTable(
                name: "Payables");
        }
    }
}
