using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Inventory.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Returns_CreatedAt",
                table: "Returns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_CreatedBy",
                table: "Returns",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_LastModifiedAt",
                table: "Returns",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_LastModifiedBy",
                table: "Returns",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnLines_CreatedAt",
                table: "ReturnLines",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnLines_CreatedBy",
                table: "ReturnLines",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnLines_LastModifiedAt",
                table: "ReturnLines",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnLines_LastModifiedBy",
                table: "ReturnLines",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_CreatedAt",
                table: "Batches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_CreatedBy",
                table: "Batches",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_LastModifiedAt",
                table: "Batches",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_LastModifiedBy",
                table: "Batches",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BatchAllocations_CreatedAt",
                table: "BatchAllocations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BatchAllocations_CreatedBy",
                table: "BatchAllocations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BatchAllocations_LastModifiedAt",
                table: "BatchAllocations",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BatchAllocations_LastModifiedBy",
                table: "BatchAllocations",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_CreatedAt",
                table: "Allocations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_CreatedBy",
                table: "Allocations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_LastModifiedAt",
                table: "Allocations",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_LastModifiedBy",
                table: "Allocations",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationLines_CreatedAt",
                table: "AllocationLines",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationLines_CreatedBy",
                table: "AllocationLines",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationLines_LastModifiedAt",
                table: "AllocationLines",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationLines_LastModifiedBy",
                table: "AllocationLines",
                column: "LastModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Returns_CreatedAt",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_CreatedBy",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_LastModifiedAt",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_LastModifiedBy",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_ReturnLines_CreatedAt",
                table: "ReturnLines");

            migrationBuilder.DropIndex(
                name: "IX_ReturnLines_CreatedBy",
                table: "ReturnLines");

            migrationBuilder.DropIndex(
                name: "IX_ReturnLines_LastModifiedAt",
                table: "ReturnLines");

            migrationBuilder.DropIndex(
                name: "IX_ReturnLines_LastModifiedBy",
                table: "ReturnLines");

            migrationBuilder.DropIndex(
                name: "IX_Batches_CreatedAt",
                table: "Batches");

            migrationBuilder.DropIndex(
                name: "IX_Batches_CreatedBy",
                table: "Batches");

            migrationBuilder.DropIndex(
                name: "IX_Batches_LastModifiedAt",
                table: "Batches");

            migrationBuilder.DropIndex(
                name: "IX_Batches_LastModifiedBy",
                table: "Batches");

            migrationBuilder.DropIndex(
                name: "IX_BatchAllocations_CreatedAt",
                table: "BatchAllocations");

            migrationBuilder.DropIndex(
                name: "IX_BatchAllocations_CreatedBy",
                table: "BatchAllocations");

            migrationBuilder.DropIndex(
                name: "IX_BatchAllocations_LastModifiedAt",
                table: "BatchAllocations");

            migrationBuilder.DropIndex(
                name: "IX_BatchAllocations_LastModifiedBy",
                table: "BatchAllocations");

            migrationBuilder.DropIndex(
                name: "IX_Allocations_CreatedAt",
                table: "Allocations");

            migrationBuilder.DropIndex(
                name: "IX_Allocations_CreatedBy",
                table: "Allocations");

            migrationBuilder.DropIndex(
                name: "IX_Allocations_LastModifiedAt",
                table: "Allocations");

            migrationBuilder.DropIndex(
                name: "IX_Allocations_LastModifiedBy",
                table: "Allocations");

            migrationBuilder.DropIndex(
                name: "IX_AllocationLines_CreatedAt",
                table: "AllocationLines");

            migrationBuilder.DropIndex(
                name: "IX_AllocationLines_CreatedBy",
                table: "AllocationLines");

            migrationBuilder.DropIndex(
                name: "IX_AllocationLines_LastModifiedAt",
                table: "AllocationLines");

            migrationBuilder.DropIndex(
                name: "IX_AllocationLines_LastModifiedBy",
                table: "AllocationLines");
        }
    }
}
