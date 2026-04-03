using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Inventory.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class BatchAllocationMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatchAllocations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BatchId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrderItemId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    QuantityAllocated = table.Column<int>(type: "int", nullable: false),
                    AllocatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    BatchId1 = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchAllocations_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatchAllocations_Batches_BatchId1",
                        column: x => x.BatchId1,
                        principalTable: "Batches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchAllocations_BatchId",
                table: "BatchAllocations",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchAllocations_BatchId1",
                table: "BatchAllocations",
                column: "BatchId1");

            migrationBuilder.CreateIndex(
                name: "IX_BatchAllocations_OrderItemId",
                table: "BatchAllocations",
                column: "OrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchAllocations");
        }
    }
}
