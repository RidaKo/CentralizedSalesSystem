using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentralizedSalesSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class NoUserInRefund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_GiftCards_GiftCardId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "Refunds",
                newName: "Amount");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_GiftCards_GiftCardId",
                table: "Payments",
                column: "GiftCardId",
                principalTable: "GiftCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_GiftCards_GiftCardId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Refunds",
                newName: "amount");

            migrationBuilder.AddForeignKey(
                name: "FK_Refunds_Users_UserId",
                table: "Refunds",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
