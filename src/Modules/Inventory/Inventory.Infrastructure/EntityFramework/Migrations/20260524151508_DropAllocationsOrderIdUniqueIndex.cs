using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Inventory.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class DropAllocationsOrderIdUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Allocations_OrderId",
                table: "Allocations");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_OrderId",
                table: "Allocations",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Allocations_OrderId",
                table: "Allocations");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_OrderId",
                table: "Allocations",
                column: "OrderId",
                unique: true);
        }
    }
}
