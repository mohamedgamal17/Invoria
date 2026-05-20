using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Reporting.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class ReportedOrderStatusByDayRollup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportedOrderStatusByDay",
                columns: table => new
                {
                    DayUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    OrderStatus = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportedOrderStatusByDay", x => new { x.DayUtc, x.OrderStatus });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportedOrderStatusByDay");
        }
    }
}
