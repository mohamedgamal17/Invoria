using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Ordering.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrderFulfillmentMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderFailureDetails");

            migrationBuilder.DropTable(
                name: "OrderStateTransitionHistory");

            migrationBuilder.DropColumn(
                name: "FullfillmentStatus",
                table: "Order");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FullfillmentStatus",
                table: "Order",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.CreateTable(
                name: "OrderFailureDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ItemId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OrderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false),
                    QuantityRequested = table.Column<int>(type: "int", nullable: false),
                    Shortage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderFailureDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderFailureDetails_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderStateTransitionHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FromFullfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ToFullfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_OrderFailureDetails_ItemId",
                table: "OrderFailureDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFailureDetails_OrderId",
                table: "OrderFailureDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStateTransitionHistory_ChangedAt",
                table: "OrderStateTransitionHistory",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStateTransitionHistory_OrderId",
                table: "OrderStateTransitionHistory",
                column: "OrderId");
        }
    }
}
