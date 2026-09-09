using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Ordering.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrderPayment_CreatedAt",
                table: "OrderPayment",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayment_CreatedBy",
                table: "OrderPayment",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayment_LastModifiedAt",
                table: "OrderPayment",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayment_LastModifiedBy",
                table: "OrderPayment",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptions_CreatedAt",
                table: "OrderAllocationConsumptions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptions_CreatedBy",
                table: "OrderAllocationConsumptions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptions_LastModifiedAt",
                table: "OrderAllocationConsumptions",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptions_LastModifiedBy",
                table: "OrderAllocationConsumptions",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionLines_CreatedAt",
                table: "OrderAllocationConsumptionLines",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionLines_CreatedBy",
                table: "OrderAllocationConsumptionLines",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionLines_LastModifiedAt",
                table: "OrderAllocationConsumptionLines",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionLines_LastModifiedBy",
                table: "OrderAllocationConsumptionLines",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionBatches_CreatedAt",
                table: "OrderAllocationConsumptionBatches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionBatches_CreatedBy",
                table: "OrderAllocationConsumptionBatches",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionBatches_LastModifiedAt",
                table: "OrderAllocationConsumptionBatches",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllocationConsumptionBatches_LastModifiedBy",
                table: "OrderAllocationConsumptionBatches",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CreatedAt",
                table: "Order",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CreatedBy",
                table: "Order",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Order_LastModifiedAt",
                table: "Order",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Order_LastModifiedBy",
                table: "Order",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CreatedAt",
                table: "Invoices",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CreatedBy",
                table: "Invoices",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_LastModifiedAt",
                table: "Invoices",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_LastModifiedBy",
                table: "Invoices",
                column: "LastModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderPayment_CreatedAt",
                table: "OrderPayment");

            migrationBuilder.DropIndex(
                name: "IX_OrderPayment_CreatedBy",
                table: "OrderPayment");

            migrationBuilder.DropIndex(
                name: "IX_OrderPayment_LastModifiedAt",
                table: "OrderPayment");

            migrationBuilder.DropIndex(
                name: "IX_OrderPayment_LastModifiedBy",
                table: "OrderPayment");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptions_CreatedAt",
                table: "OrderAllocationConsumptions");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptions_CreatedBy",
                table: "OrderAllocationConsumptions");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptions_LastModifiedAt",
                table: "OrderAllocationConsumptions");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptions_LastModifiedBy",
                table: "OrderAllocationConsumptions");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptionLines_CreatedAt",
                table: "OrderAllocationConsumptionLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptionLines_CreatedBy",
                table: "OrderAllocationConsumptionLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptionLines_LastModifiedAt",
                table: "OrderAllocationConsumptionLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptionLines_LastModifiedBy",
                table: "OrderAllocationConsumptionLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptionBatches_CreatedAt",
                table: "OrderAllocationConsumptionBatches");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptionBatches_CreatedBy",
                table: "OrderAllocationConsumptionBatches");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptionBatches_LastModifiedAt",
                table: "OrderAllocationConsumptionBatches");

            migrationBuilder.DropIndex(
                name: "IX_OrderAllocationConsumptionBatches_LastModifiedBy",
                table: "OrderAllocationConsumptionBatches");

            migrationBuilder.DropIndex(
                name: "IX_Order_CreatedAt",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_CreatedBy",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_LastModifiedAt",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_LastModifiedBy",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_CreatedAt",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_CreatedBy",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_LastModifiedAt",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_LastModifiedBy",
                table: "Invoices");
        }
    }
}
