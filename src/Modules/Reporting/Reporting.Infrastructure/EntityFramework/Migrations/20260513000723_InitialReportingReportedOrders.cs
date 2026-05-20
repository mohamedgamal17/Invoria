using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Reporting.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialReportingReportedOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportedOrder",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrderStatus = table.Column<int>(type: "int", nullable: false),
                    FullfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    PaymentType = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    TotalOrderAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountOutstanding = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReplicationVersion = table.Column<long>(type: "bigint", nullable: false),
                    SourceLastKnownAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportedOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportedOrderFailureDetail",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReportedOrderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<int>(type: "int", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false),
                    Shortage = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
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
                name: "ReportedOrderLine",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReportedOrderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportedOrderLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportedOrderLine_ReportedOrder_ReportedOrderId",
                        column: x => x.ReportedOrderId,
                        principalTable: "ReportedOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportedOrderPayment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReportedOrderId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportedOrderPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportedOrderPayment_ReportedOrder_ReportedOrderId",
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
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    FromFullfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    ToFullfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
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
                name: "IX_ReportedOrder_CustomerId",
                table: "ReportedOrder",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportedOrderFailureDetail_ItemId",
                table: "ReportedOrderFailureDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportedOrderFailureDetail_ReportedOrderId",
                table: "ReportedOrderFailureDetail",
                column: "ReportedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportedOrderLine_ProductId",
                table: "ReportedOrderLine",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportedOrderLine_ReportedOrderId",
                table: "ReportedOrderLine",
                column: "ReportedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportedOrderPayment_ReportedOrderId",
                table: "ReportedOrderPayment",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportedOrderFailureDetail");

            migrationBuilder.DropTable(
                name: "ReportedOrderLine");

            migrationBuilder.DropTable(
                name: "ReportedOrderPayment");

            migrationBuilder.DropTable(
                name: "ReportedOrderStateTransition");

            migrationBuilder.DropTable(
                name: "ReportedOrder");
        }
    }
}
