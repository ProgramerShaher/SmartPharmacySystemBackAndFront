using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditPuerchesivoiceandMedicine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantityInSaleUnit",
                table: "SaleInvoiceDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SaleUnitId",
                table: "SaleInvoiceDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseUnitId",
                table: "PurchaseInvoiceDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityInPurchaseUnit",
                table: "PurchaseInvoiceDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllowedForPurchase",
                table: "MedicineUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllowedForSale",
                table: "MedicineUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "MedicineUnits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 31, 712, DateTimeKind.Utc).AddTicks(9849));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 31, 713, DateTimeKind.Utc).AddTicks(1151));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 31, 713, DateTimeKind.Utc).AddTicks(1154));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 31, 713, DateTimeKind.Utc).AddTicks(1156));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 31, 713, DateTimeKind.Utc).AddTicks(1157));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 31, 713, DateTimeKind.Utc).AddTicks(1158));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 31, 713, DateTimeKind.Utc).AddTicks(1159));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 31, 713, DateTimeKind.Utc).AddTicks(1160));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 31, 713, DateTimeKind.Utc).AddTicks(1162));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 32, 396, DateTimeKind.Utc).AddTicks(1481));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 25, 23, 4, 32, 396, DateTimeKind.Utc).AddTicks(6558));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$7.Vk.hx20KZpbsVJK0aiauzg6uSxovMnURHmKddVBY6PIKivIUxtG");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$7.Vk.hx20KZpbsVJK0aiauzg6uSxovMnURHmKddVBY6PIKivIUxtG");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoiceDetails_SaleUnitId",
                table: "SaleInvoiceDetails",
                column: "SaleUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetails_PurchaseUnitId",
                table: "PurchaseInvoiceDetails",
                column: "PurchaseUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceDetails_MedicineUnits_PurchaseUnitId",
                table: "PurchaseInvoiceDetails",
                column: "PurchaseUnitId",
                principalTable: "MedicineUnits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoiceDetails_MedicineUnits_SaleUnitId",
                table: "SaleInvoiceDetails",
                column: "SaleUnitId",
                principalTable: "MedicineUnits",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceDetails_MedicineUnits_PurchaseUnitId",
                table: "PurchaseInvoiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoiceDetails_MedicineUnits_SaleUnitId",
                table: "SaleInvoiceDetails");

            migrationBuilder.DropIndex(
                name: "IX_SaleInvoiceDetails_SaleUnitId",
                table: "SaleInvoiceDetails");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceDetails_PurchaseUnitId",
                table: "PurchaseInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "QuantityInSaleUnit",
                table: "SaleInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "SaleUnitId",
                table: "SaleInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "PurchaseUnitId",
                table: "PurchaseInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "QuantityInPurchaseUnit",
                table: "PurchaseInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "IsAllowedForPurchase",
                table: "MedicineUnits");

            migrationBuilder.DropColumn(
                name: "IsAllowedForSale",
                table: "MedicineUnits");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "MedicineUnits");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(129));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2516));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2521));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2523));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2527));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2708));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2710));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2712));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2714));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 22, 79, DateTimeKind.Utc).AddTicks(8584));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 22, 81, DateTimeKind.Utc).AddTicks(2273));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Tq8UmC7naRaCdQpMq6.NLO8PxIwXML9Armd5gQhTZCk9wJ5w9Uywq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$Tq8UmC7naRaCdQpMq6.NLO8PxIwXML9Armd5gQhTZCk9wJ5w9Uywq");
        }
    }
}
