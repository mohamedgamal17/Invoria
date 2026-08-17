using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Ordering.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddReportOrderSalesProfitMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportOrderSalesProfitMetrics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalReturnAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportOrderSalesProfitMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportOrderSalesProfitMetrics_Date",
                table: "ReportOrderSalesProfitMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ReportOrderSalesProfitMetrics_Period_Date",
                table: "ReportOrderSalesProfitMetrics",
                columns: new[] { "Period", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportOrderSalesProfitMetrics");
        }
    }
}
