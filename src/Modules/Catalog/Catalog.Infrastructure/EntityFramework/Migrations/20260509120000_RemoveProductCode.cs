using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Catalog.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Product");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Product",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
