using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.CustomerManagement.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddReportDateToReportCustomerMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Date",
                table: "ReportCustomerMetrics",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_ReportCustomerMetrics_Date",
                table: "ReportCustomerMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ReportCustomerMetrics_Period_Date",
                table: "ReportCustomerMetrics",
                columns: new[] { "Period", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportCustomerMetrics_Date",
                table: "ReportCustomerMetrics");

            migrationBuilder.DropIndex(
                name: "IX_ReportCustomerMetrics_Period_Date",
                table: "ReportCustomerMetrics");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "ReportCustomerMetrics");
        }
    }
}
