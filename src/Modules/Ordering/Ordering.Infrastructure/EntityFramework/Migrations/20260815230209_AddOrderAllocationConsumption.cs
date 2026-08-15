using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Ordering.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderAllocationConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderAllocationConsumptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AllocationId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAllocationConsumptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderAllocationConsumptionLines",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ConsumptionId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrderItemId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAllocationConsumptionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderAllocationConsumptionLines_OrderAllocationConsumptions_ConsumptionId",
                        column: x => x.ConsumptionId,
                        principalTable: "OrderAllocationConsumptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderAllocationConsumptionBatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ConsumptionLineId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BatchId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAllocationConsumptionBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderAllocationConsumptionBatches_OrderAllocationConsumptionLines_ConsumptionLineId",
                        column: x => x.ConsumptionLineId,
                        principalTable: "OrderAllocationConsumptionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionBatches_BatchId",
                table: "OrderAllocationConsumptionBatches",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionBatches_ConsumptionLineId",
                table: "OrderAllocationConsumptionBatches",
                column: "ConsumptionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionLines_ConsumptionId",
                table: "OrderAllocationConsumptionLines",
                column: "ConsumptionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionLines_OrderItemId",
                table: "OrderAllocationConsumptionLines",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptions_AllocationId",
                table: "OrderAllocationConsumptions",
                column: "AllocationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptions_OrderId",
                table: "OrderAllocationConsumptions",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderAllocationConsumptionBatches");

            migrationBuilder.DropTable(
                name: "OrderAllocationConsumptionLines");

            migrationBuilder.DropTable(
                name: "OrderAllocationConsumptions");
        }
    }
}
