using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentralizedSalesSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Addedgiftcard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "paidAt",
                table: "Payments",
                newName: "PaidAt");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "Payments",
                newName: "Amount");

            migrationBuilder.AddColumn<long>(
                name: "BusinessId",
                table: "Roles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GiftCardId",
                table: "Payments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Businesses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    SubscriptionPlan = table.Column<int>(type: "int", nullable: false),
                    NextPaymentDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkingHours = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Businesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Businesses_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefundedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefundMethod = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Refunds_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GiftCards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InitialValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IssuedBy = table.Column<long>(type: "bigint", nullable: false),
                    IssuedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BusinessId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiftCards_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_BusinessId",
                table: "Users",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_BusinessId",
                table: "Taxes",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_BusinessId",
                table: "Tables",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCharges_BusinessId",
                table: "ServiceCharges",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_BusinessId",
                table: "Roles",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BusinessId",
                table: "Reservations",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GiftCardId",
                table: "Payments",
                column: "GiftCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BusinessId",
                table: "Orders",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_BusinessId",
                table: "Items",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_BusinessId",
                table: "Discounts",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_OwnerId",
                table: "Businesses",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_BusinessId",
                table: "GiftCards",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_OrderId",
                table: "Refunds",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_PaymentId",
                table: "Refunds",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_UserId",
                table: "Refunds",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_Businesses_BusinessId",
                table: "Discounts",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Businesses_BusinessId",
                table: "Items",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Businesses_BusinessId",
                table: "Orders",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_GiftCards_GiftCardId",
                table: "Payments",
                column: "GiftCardId",
                principalTable: "GiftCards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Businesses_BusinessId",
                table: "Reservations",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Businesses_BusinessId",
                table: "Roles",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCharges_Businesses_BusinessId",
                table: "ServiceCharges",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_Businesses_BusinessId",
                table: "Tables",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Taxes_Businesses_BusinessId",
                table: "Taxes",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Businesses_BusinessId",
                table: "Users",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discounts_Businesses_BusinessId",
                table: "Discounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Businesses_BusinessId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Businesses_BusinessId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_GiftCards_GiftCardId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Businesses_BusinessId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Businesses_BusinessId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCharges_Businesses_BusinessId",
                table: "ServiceCharges");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_Businesses_BusinessId",
                table: "Tables");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxes_Businesses_BusinessId",
                table: "Taxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Businesses_BusinessId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "GiftCards");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Users_BusinessId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_BusinessId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Tables_BusinessId",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCharges_BusinessId",
                table: "ServiceCharges");

            migrationBuilder.DropIndex(
                name: "IX_Roles_BusinessId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BusinessId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Payments_GiftCardId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BusinessId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Items_BusinessId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Discounts_BusinessId",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "GiftCardId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "PaidAt",
                table: "Payments",
                newName: "paidAt");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Payments",
                newName: "amount");
        }
    }
}
