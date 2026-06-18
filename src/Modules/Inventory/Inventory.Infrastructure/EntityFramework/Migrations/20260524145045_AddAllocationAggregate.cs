using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Inventory.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddAllocationAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllocationLineId",
                table: "BatchAllocations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Allocations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AllocationLines",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AllocationId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrderItemId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllocationLines_Allocations_AllocationId",
                        column: x => x.AllocationId,
                        principalTable: "Allocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchAllocations_AllocationLineId",
                table: "BatchAllocations",
                column: "AllocationLineId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationLines_AllocationId",
                table: "AllocationLines",
                column: "AllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationLines_OrderItemId",
                table: "AllocationLines",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_OrderId",
                table: "Allocations",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BatchAllocations_AllocationLines_AllocationLineId",
                table: "BatchAllocations",
                column: "AllocationLineId",
                principalTable: "AllocationLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchAllocations_AllocationLines_AllocationLineId",
                table: "BatchAllocations");

            migrationBuilder.DropTable(
                name: "AllocationLines");

            migrationBuilder.DropTable(
                name: "Allocations");

            migrationBuilder.DropIndex(
                name: "IX_BatchAllocations_AllocationLineId",
                table: "BatchAllocations");

            migrationBuilder.DropColumn(
                name: "AllocationLineId",
                table: "BatchAllocations");
        }
    }
}
