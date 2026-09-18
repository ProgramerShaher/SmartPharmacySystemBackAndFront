using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleInvoicePaymentsAndReceiptLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "SaleInvoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SaleInvoiceId",
                table: "CustomerReceipts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SaleInvoicePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleInvoiceId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleInvoicePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleInvoicePayments_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleInvoicePayments_SaleInvoices_SaleInvoiceId",
                        column: x => x.SaleInvoiceId,
                        principalTable: "SaleInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 51, 943, DateTimeKind.Utc).AddTicks(1970));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 51, 943, DateTimeKind.Utc).AddTicks(3157));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 51, 943, DateTimeKind.Utc).AddTicks(3160));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 51, 943, DateTimeKind.Utc).AddTicks(3163));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 51, 943, DateTimeKind.Utc).AddTicks(3165));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 51, 943, DateTimeKind.Utc).AddTicks(3167));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 51, 943, DateTimeKind.Utc).AddTicks(3168));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 51, 943, DateTimeKind.Utc).AddTicks(3170));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 51, 943, DateTimeKind.Utc).AddTicks(3172));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 52, 748, DateTimeKind.Utc).AddTicks(8956));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 5, 43, 52, 749, DateTimeKind.Utc).AddTicks(333));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$59guL6ctWGKTKWEtlTn6BOO9Xna5TxaDlcp.S4lwIvb5Hj8ZbcwR6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$59guL6ctWGKTKWEtlTn6BOO9Xna5TxaDlcp.S4lwIvb5Hj8ZbcwR6");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceipts_SaleInvoiceId",
                table: "CustomerReceipts",
                column: "SaleInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoicePayments_AccountId",
                table: "SaleInvoicePayments",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoicePayments_SaleInvoiceId",
                table: "SaleInvoicePayments",
                column: "SaleInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerReceipts_SaleInvoices_SaleInvoiceId",
                table: "CustomerReceipts",
                column: "SaleInvoiceId",
                principalTable: "SaleInvoices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerReceipts_SaleInvoices_SaleInvoiceId",
                table: "CustomerReceipts");

            migrationBuilder.DropTable(
                name: "SaleInvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_CustomerReceipts_SaleInvoiceId",
                table: "CustomerReceipts");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "SaleInvoices");

            migrationBuilder.DropColumn(
                name: "SaleInvoiceId",
                table: "CustomerReceipts");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 49, 926, DateTimeKind.Utc).AddTicks(9641));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 49, 927, DateTimeKind.Utc).AddTicks(659));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 49, 927, DateTimeKind.Utc).AddTicks(661));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 49, 927, DateTimeKind.Utc).AddTicks(662));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 49, 927, DateTimeKind.Utc).AddTicks(664));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 49, 927, DateTimeKind.Utc).AddTicks(665));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 49, 927, DateTimeKind.Utc).AddTicks(666));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 49, 927, DateTimeKind.Utc).AddTicks(701));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 49, 927, DateTimeKind.Utc).AddTicks(703));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 50, 651, DateTimeKind.Utc).AddTicks(2145));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 3, 56, 50, 651, DateTimeKind.Utc).AddTicks(3426));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$2.eDaoGG6l0wRpPzJ6FDmucpKiIhc5XemeDPrW4xpLkB4iGMxwqxy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$2.eDaoGG6l0wRpPzJ6FDmucpKiIhc5XemeDPrW4xpLkB4iGMxwqxy");
        }
    }
}
