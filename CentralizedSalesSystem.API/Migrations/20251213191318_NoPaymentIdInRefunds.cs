using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentralizedSalesSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class NoPaymentIdInRefunds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Refunds");*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {/*
            migrationBuilder.AddColumn<long>(
                name: "PaymentId",
                table: "Refunds",
                type: "bigint",
                nullable: true);
        */
        }
    }
}
