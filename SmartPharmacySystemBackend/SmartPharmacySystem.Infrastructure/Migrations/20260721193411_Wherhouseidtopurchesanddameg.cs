using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Wherhouseidtopurchesanddameg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "PurchaseInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "CurrentBalance", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "IsMainAccount", "Name", "ParentId", "Type", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 5206, "5206", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0m, null, null, null, true, false, false, "خسائر الأدوية التالفة", 52, 5, null, null });

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 1, 199, DateTimeKind.Utc).AddTicks(7862));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 1, 199, DateTimeKind.Utc).AddTicks(9862));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 1, 199, DateTimeKind.Utc).AddTicks(9867));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 1, 199, DateTimeKind.Utc).AddTicks(9869));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 1, 199, DateTimeKind.Utc).AddTicks(9871));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 1, 199, DateTimeKind.Utc).AddTicks(9873));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 1, 199, DateTimeKind.Utc).AddTicks(9874));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 1, 199, DateTimeKind.Utc).AddTicks(9876));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 1, 199, DateTimeKind.Utc).AddTicks(9877));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 2, 186, DateTimeKind.Utc).AddTicks(9656));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 21, 19, 34, 2, 187, DateTimeKind.Utc).AddTicks(4022));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$JUra8LNKMcGKkPzD2cZXg.7quhNaanr6YWNzRz07hfM0w4uTb.Mby");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$JUra8LNKMcGKkPzD2cZXg.7quhNaanr6YWNzRz07hfM0w4uTb.Mby");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_WarehouseId",
                table: "PurchaseInvoices",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Warehouses_WarehouseId",
                table: "PurchaseInvoices",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Warehouses_WarehouseId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_WarehouseId",
                table: "PurchaseInvoices");

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 5206);

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "PurchaseInvoices");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 56, 566, DateTimeKind.Utc).AddTicks(7407));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 56, 567, DateTimeKind.Utc).AddTicks(7375));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 56, 567, DateTimeKind.Utc).AddTicks(7385));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 56, 567, DateTimeKind.Utc).AddTicks(7389));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 56, 567, DateTimeKind.Utc).AddTicks(7392));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 56, 567, DateTimeKind.Utc).AddTicks(7395));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 56, 567, DateTimeKind.Utc).AddTicks(7399));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 56, 567, DateTimeKind.Utc).AddTicks(7401));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 56, 567, DateTimeKind.Utc).AddTicks(7403));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 57, 704, DateTimeKind.Utc).AddTicks(9482));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 23, 22, 57, 705, DateTimeKind.Utc).AddTicks(3928));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$qZapA5AjV3wBwg9rEXL.xOtnCHm.KYAwGVfbobPqpt9OqLNZuhNXO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$qZapA5AjV3wBwg9rEXL.xOtnCHm.KYAwGVfbobPqpt9OqLNZuhNXO");
        }
    }
}
