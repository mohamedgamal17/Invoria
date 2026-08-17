using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Procurement.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddReportSupplierMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportSupplierMetrics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalCount = table.Column<long>(type: "bigint", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSupplierMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportSupplierMetrics_Date",
                table: "ReportSupplierMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSupplierMetrics_Period_Date",
                table: "ReportSupplierMetrics",
                columns: new[] { "Period", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportSupplierMetrics");
        }
    }
}
