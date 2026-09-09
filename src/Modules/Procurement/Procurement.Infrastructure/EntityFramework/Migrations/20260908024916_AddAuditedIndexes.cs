using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Procurement.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CreatedAt",
                table: "Suppliers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CreatedBy",
                table: "Suppliers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_LastModifiedAt",
                table: "Suppliers",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_LastModifiedBy",
                table: "Suppliers",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderSequences_CreatedAt",
                table: "PurchaseOrderSequences",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderSequences_CreatedBy",
                table: "PurchaseOrderSequences",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderSequences_LastModifiedAt",
                table: "PurchaseOrderSequences",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderSequences_LastModifiedBy",
                table: "PurchaseOrderSequences",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CreatedAt",
                table: "PurchaseOrders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CreatedBy",
                table: "PurchaseOrders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_LastModifiedAt",
                table: "PurchaseOrders",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_LastModifiedBy",
                table: "PurchaseOrders",
                column: "LastModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suppliers_CreatedAt",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_CreatedBy",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_LastModifiedAt",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_LastModifiedBy",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderSequences_CreatedAt",
                table: "PurchaseOrderSequences");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderSequences_CreatedBy",
                table: "PurchaseOrderSequences");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderSequences_LastModifiedAt",
                table: "PurchaseOrderSequences");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderSequences_LastModifiedBy",
                table: "PurchaseOrderSequences");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_CreatedAt",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_CreatedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_LastModifiedAt",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_LastModifiedBy",
                table: "PurchaseOrders");
        }
    }
}
