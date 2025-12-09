using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentralizedSalesSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxAndServiceChargeToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
            name: "TaxId",
            table: "OrderItems",
            type: "bigint",
            nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ServiceChargeId",
                table: "OrderItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_TaxId",
                table: "OrderItems",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ServiceChargeId",
                table: "OrderItems",
                column: "ServiceChargeId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
            name: "IX_OrderItems_TaxId",
            table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ServiceChargeId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ServiceChargeId",
                table: "OrderItems");

        }
    }
}
