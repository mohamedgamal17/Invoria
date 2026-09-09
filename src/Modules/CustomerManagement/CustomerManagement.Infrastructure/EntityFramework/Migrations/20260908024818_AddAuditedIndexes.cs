using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.CustomerManagement.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Customer_CreatedAt",
                table: "Customer",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CreatedBy",
                table: "Customer",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_LastModifiedAt",
                table: "Customer",
                column: "LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_LastModifiedBy",
                table: "Customer",
                column: "LastModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customer_CreatedAt",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_CreatedBy",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_LastModifiedAt",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_LastModifiedBy",
                table: "Customer");
        }
    }
}
