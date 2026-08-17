using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Ordering.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddReportOrderSalesMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportOrderSalesMetrics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalReturnAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportOrderSalesMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportOrderSalesMetrics_Date",
                table: "ReportOrderSalesMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ReportOrderSalesMetrics_Period_Date",
                table: "ReportOrderSalesMetrics",
                columns: new[] { "Period", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportOrderSalesMetrics");
        }
    }
}
