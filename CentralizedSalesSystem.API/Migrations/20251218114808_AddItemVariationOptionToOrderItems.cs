using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentralizedSalesSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddItemVariationOptionToOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ItemVariationOptionId",
                table: "OrderItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ItemVariationOptionId",
                table: "OrderItems",
                column: "ItemVariationOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ItemVariationOptions_ItemVariationOptionId",
                table: "OrderItems",
                column: "ItemVariationOptionId",
                principalTable: "ItemVariationOptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ItemVariationOptions_ItemVariationOptionId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ItemVariationOptionId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ItemVariationOptionId",
                table: "OrderItems");
        }
    }
}
