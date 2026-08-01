using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Catalog.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddReportProductMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportProductMetrics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TotalCount = table.Column<long>(type: "bigint", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportProductMetrics", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportProductMetrics");
        }
    }
}
