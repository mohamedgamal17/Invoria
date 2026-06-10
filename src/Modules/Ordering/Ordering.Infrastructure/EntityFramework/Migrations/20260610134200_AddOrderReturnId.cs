using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Ordering.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderReturnId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "Order",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "Order");
        }
    }
}
