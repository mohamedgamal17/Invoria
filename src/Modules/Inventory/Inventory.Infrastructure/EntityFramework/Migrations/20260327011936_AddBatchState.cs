using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Inventory.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Batches",
                type: "int",
                nullable: false,
                defaultValue: 20);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Batches");
        }
    }
}
