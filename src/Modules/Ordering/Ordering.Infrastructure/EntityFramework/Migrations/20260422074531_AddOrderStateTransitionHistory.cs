using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Ordering.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStateTransitionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderStateTransitionHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    FromFullfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    ToFullfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStateTransitionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStateTransitionHistory_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderStateTransitionHistory_ChangedAt",
                table: "OrderStateTransitionHistory",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStateTransitionHistory_OrderId",
                table: "OrderStateTransitionHistory",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderStateTransitionHistory");
        }
    }
}
