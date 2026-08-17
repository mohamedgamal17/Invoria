using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Procurement.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddReportPurchaseSalesMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportPurchaseSalesMetrics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportPurchaseSalesMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportPurchaseSalesMetrics_Date",
                table: "ReportPurchaseSalesMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ReportPurchaseSalesMetrics_Period_Date",
                table: "ReportPurchaseSalesMetrics",
                columns: new[] { "Period", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportPurchaseSalesMetrics");
        }
    }
}
