using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Catalog.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Product_CreatedAt",
                table: "Product",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CreatedBy",
                table: "Product",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Product_LastModifiedAt",
                table: "Product",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Product_LastModifiedBy",
                table: "Product",
                column: "LastModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Product_CreatedAt",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_CreatedBy",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_LastModifiedAt",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_LastModifiedBy",
                table: "Product");
        }
    }
}
