using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVATAndZatcaEInvoicing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTaxInclusive",
                table: "SaleInvoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "SaleInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "SaleInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "SaleInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ZatcaQrCode",
                table: "SaleInvoices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "SaleInvoiceDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "SaleInvoiceDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "SaleInvoiceDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaxInclusive",
                table: "PurchaseInvoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "PurchaseInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "PurchaseInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "PurchaseInvoiceDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "PurchaseInvoiceDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "PurchaseInvoiceDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 43, 352, DateTimeKind.Utc).AddTicks(253));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 43, 352, DateTimeKind.Utc).AddTicks(1485));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 43, 352, DateTimeKind.Utc).AddTicks(1488));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 43, 352, DateTimeKind.Utc).AddTicks(1492));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 43, 352, DateTimeKind.Utc).AddTicks(1494));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 43, 352, DateTimeKind.Utc).AddTicks(1496));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 43, 352, DateTimeKind.Utc).AddTicks(1497));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 43, 352, DateTimeKind.Utc).AddTicks(1500));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 43, 352, DateTimeKind.Utc).AddTicks(1501));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 44, 355, DateTimeKind.Utc).AddTicks(41));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 56, 44, 355, DateTimeKind.Utc).AddTicks(1689));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$2FJYcXOVUm24Z9TtJbeiQ.5xcUUVh8ciiwUICI7B/UTb1SP8LiKdK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$2FJYcXOVUm24Z9TtJbeiQ.5xcUUVh8ciiwUICI7B/UTb1SP8LiKdK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTaxInclusive",
                table: "SaleInvoices");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "SaleInvoices");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "SaleInvoices");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "SaleInvoices");

            migrationBuilder.DropColumn(
                name: "ZatcaQrCode",
                table: "SaleInvoices");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "SaleInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "SaleInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "SaleInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "IsTaxInclusive",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "PurchaseInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "PurchaseInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "PurchaseInvoiceDetails");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(3228));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4486));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4490));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4492));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4494));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4561));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4563));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4565));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4567));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 7, 940, DateTimeKind.Utc).AddTicks(3950));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 7, 940, DateTimeKind.Utc).AddTicks(6188));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$dTbRyYphztMRyJ2veaGD1.wfrNIIuCHYx1swcwyy5.TqDfgpDn.vi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$dTbRyYphztMRyJ2veaGD1.wfrNIIuCHYx1swcwyy5.TqDfgpDn.vi");
        }
    }
}
