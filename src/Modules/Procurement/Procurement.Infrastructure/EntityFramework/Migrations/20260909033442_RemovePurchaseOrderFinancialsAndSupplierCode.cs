using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Procurement.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RemovePurchaseOrderFinancialsAndSupplierCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "SupplierProductCode",
                table: "PurchaseOrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBatchIds",
                table: "PurchaseOrderItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "PurchaseOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "PurchaseOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBatchIds",
                table: "PurchaseOrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierProductCode",
                table: "PurchaseOrderItems",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
