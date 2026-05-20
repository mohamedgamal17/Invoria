using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoria.Ordering.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RenumberOrderStatusForShipped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            RenumberOrderStatusUp(migrationBuilder, "Order", "Status");
            RenumberOrderStatusUp(migrationBuilder, "OrderStateTransitionHistory", "FromStatus");
            RenumberOrderStatusUp(migrationBuilder, "OrderStateTransitionHistory", "ToStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            RenumberOrderStatusDown(migrationBuilder, "Order", "Status");
            RenumberOrderStatusDown(migrationBuilder, "OrderStateTransitionHistory", "FromStatus");
            RenumberOrderStatusDown(migrationBuilder, "OrderStateTransitionHistory", "ToStatus");
        }

        private static void RenumberOrderStatusUp(MigrationBuilder migrationBuilder, string table, string column)
        {
            migrationBuilder.Sql($"""
                UPDATE [{table}] SET [{column}] = 35 WHERE [{column}] = 30;
                UPDATE [{table}] SET [{column}] = 30 WHERE [{column}] = 25;
                UPDATE [{table}] SET [{column}] = 25 WHERE [{column}] = 20;
                UPDATE [{table}] SET [{column}] = 20 WHERE [{column}] = 15;
                UPDATE [{table}] SET [{column}] = 15 WHERE [{column}] = 12;
                """);
        }

        private static void RenumberOrderStatusDown(MigrationBuilder migrationBuilder, string table, string column)
        {
            migrationBuilder.Sql($"""
                UPDATE [{table}] SET [{column}] = 12 WHERE [{column}] = 15;
                UPDATE [{table}] SET [{column}] = 15 WHERE [{column}] = 20;
                UPDATE [{table}] SET [{column}] = 20 WHERE [{column}] = 25;
                UPDATE [{table}] SET [{column}] = 25 WHERE [{column}] = 30;
                UPDATE [{table}] SET [{column}] = 30 WHERE [{column}] = 35;
                """);
        }
    }
}
