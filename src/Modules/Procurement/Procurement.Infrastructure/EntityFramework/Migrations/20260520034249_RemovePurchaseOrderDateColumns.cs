using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Procurement.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RemovePurchaseOrderDateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedDeliveryDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "PurchaseOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedDeliveryDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);
        }
    }
}
