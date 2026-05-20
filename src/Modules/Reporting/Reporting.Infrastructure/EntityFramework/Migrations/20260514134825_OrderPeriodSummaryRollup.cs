using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Reporting.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class OrderPeriodSummaryRollup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderPeriodSummary",
                columns: table => new
                {
                    Granularity = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    PeriodKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DateField = table.Column<int>(type: "int", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    OrderCount = table.Column<int>(type: "int", nullable: false),
                    GrossRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CancelledCount = table.Column<int>(type: "int", nullable: false),
                    DeliveredCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPeriodSummary", x => new { x.Granularity, x.PeriodKey, x.DateField });
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderPeriodSummary_DateField_Granularity_PeriodStart",
                table: "OrderPeriodSummary",
                columns: new[] { "DateField", "Granularity", "PeriodStart" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderPeriodSummary");
        }
    }
}
