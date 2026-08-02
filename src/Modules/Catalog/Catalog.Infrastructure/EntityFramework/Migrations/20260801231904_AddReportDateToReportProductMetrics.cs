using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Catalog.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddReportDateToReportProductMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Date",
                table: "ReportProductMetrics",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_ReportProductMetrics_Date",
                table: "ReportProductMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ReportProductMetrics_Period_Date",
                table: "ReportProductMetrics",
                columns: new[] { "Period", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportProductMetrics_Date",
                table: "ReportProductMetrics");

            migrationBuilder.DropIndex(
                name: "IX_ReportProductMetrics_Period_Date",
                table: "ReportProductMetrics");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "ReportProductMetrics");
        }
    }
}
