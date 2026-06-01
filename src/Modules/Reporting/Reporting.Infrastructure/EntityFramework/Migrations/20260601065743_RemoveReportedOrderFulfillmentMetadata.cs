using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Reporting.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReportedOrderFulfillmentMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportedOrderFailureDetail");

            migrationBuilder.DropTable(
                name: "ReportedOrderStateTransition");

            migrationBuilder.DropColumn(
                name: "FullfillmentStatus",
                table: "ReportedOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FullfillmentStatus",
                table: "ReportedOrder",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ReportedOrderFailureDetail",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReportedOrderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ItemId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false),
                    QuantityRequested = table.Column<int>(type: "int", nullable: false),
                    Shortage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportedOrderFailureDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportedOrderFailureDetail_ReportedOrder_ReportedOrderId",
                        column: x => x.ReportedOrderId,
                        principalTable: "ReportedOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportedOrderStateTransition",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReportedOrderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FromFullfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ToFullfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportedOrderStateTransition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportedOrderStateTransition_ReportedOrder_ReportedOrderId",
                        column: x => x.ReportedOrderId,
                        principalTable: "ReportedOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportedOrderFailureDetail_ItemId",
                table: "ReportedOrderFailureDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportedOrderFailureDetail_ReportedOrderId",
                table: "ReportedOrderFailureDetail",
                column: "ReportedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportedOrderStateTransition_ChangedAt",
                table: "ReportedOrderStateTransition",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportedOrderStateTransition_ReportedOrderId",
                table: "ReportedOrderStateTransition",
                column: "ReportedOrderId");
        }
    }
}
