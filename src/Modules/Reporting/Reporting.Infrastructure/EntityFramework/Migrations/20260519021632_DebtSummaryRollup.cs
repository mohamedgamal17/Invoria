using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Reporting.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class DebtSummaryRollup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DebtSummary",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SummaryType = table.Column<int>(type: "int", nullable: false),
                    TotalOutstanding = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOrderValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DebtOrderCount = table.Column<int>(type: "int", nullable: false),
                    PartiallyPaidCount = table.Column<int>(type: "int", nullable: false),
                    UnpaidCount = table.Column<int>(type: "int", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OldestDebtDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    OldestDebtAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebtSummary", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DebtSummary_CustomerId",
                table: "DebtSummary",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DebtSummary");
        }
    }
}
