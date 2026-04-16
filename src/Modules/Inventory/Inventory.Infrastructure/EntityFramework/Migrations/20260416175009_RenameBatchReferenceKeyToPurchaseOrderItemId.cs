using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Inventory.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RenameBatchReferenceKeyToPurchaseOrderItemId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReferenceKey",
                table: "Batches",
                newName: "PurchaseOrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PurchaseOrderItemId",
                table: "Batches",
                newName: "ReferenceKey");
        }
    }
}
