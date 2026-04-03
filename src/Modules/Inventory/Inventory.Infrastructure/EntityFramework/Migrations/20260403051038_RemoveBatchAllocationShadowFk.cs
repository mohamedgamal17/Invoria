using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Inventory.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBatchAllocationShadowFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchAllocations_Batches_BatchId1",
                table: "BatchAllocations");

            migrationBuilder.DropIndex(
                name: "IX_BatchAllocations_BatchId1",
                table: "BatchAllocations");

            migrationBuilder.DropColumn(
                name: "BatchId1",
                table: "BatchAllocations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatchId1",
                table: "BatchAllocations",
                type: "nvarchar(256)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchAllocations_BatchId1",
                table: "BatchAllocations",
                column: "BatchId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchAllocations_Batches_BatchId1",
                table: "BatchAllocations",
                column: "BatchId1",
                principalTable: "Batches",
                principalColumn: "Id");
        }
    }
}
