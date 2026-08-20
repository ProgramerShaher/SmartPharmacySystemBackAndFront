using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftToSalesAndReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserShiftId",
                table: "SalesReturns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserShiftId",
                table: "SaleInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 22, 814, DateTimeKind.Utc).AddTicks(6594));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 22, 814, DateTimeKind.Utc).AddTicks(8931));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 22, 814, DateTimeKind.Utc).AddTicks(8935));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 22, 814, DateTimeKind.Utc).AddTicks(8937));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 22, 814, DateTimeKind.Utc).AddTicks(8940));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 22, 814, DateTimeKind.Utc).AddTicks(8942));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 22, 814, DateTimeKind.Utc).AddTicks(8944));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 22, 814, DateTimeKind.Utc).AddTicks(8946));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 22, 814, DateTimeKind.Utc).AddTicks(8948));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 25, 145, DateTimeKind.Utc).AddTicks(7654));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 19, 56, 25, 146, DateTimeKind.Utc).AddTicks(2793));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$WbVIA2U2L/fzj4FC36DULe4OzI3GiFbWcmag0uBWrdlpuuyEr7ZlG");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$WbVIA2U2L/fzj4FC36DULe4OzI3GiFbWcmag0uBWrdlpuuyEr7ZlG");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_UserShiftId",
                table: "SalesReturns",
                column: "UserShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoices_UserShiftId",
                table: "SaleInvoices",
                column: "UserShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoices_UserShifts_UserShiftId",
                table: "SaleInvoices",
                column: "UserShiftId",
                principalTable: "UserShifts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_UserShifts_UserShiftId",
                table: "SalesReturns",
                column: "UserShiftId",
                principalTable: "UserShifts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoices_UserShifts_UserShiftId",
                table: "SaleInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_UserShifts_UserShiftId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_UserShiftId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SaleInvoices_UserShiftId",
                table: "SaleInvoices");

            migrationBuilder.DropColumn(
                name: "UserShiftId",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "UserShiftId",
                table: "SaleInvoices");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 15, 402, DateTimeKind.Utc).AddTicks(3969));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 15, 402, DateTimeKind.Utc).AddTicks(6023));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 15, 402, DateTimeKind.Utc).AddTicks(6027));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 15, 402, DateTimeKind.Utc).AddTicks(6029));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 15, 402, DateTimeKind.Utc).AddTicks(6030));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 15, 402, DateTimeKind.Utc).AddTicks(6032));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 15, 402, DateTimeKind.Utc).AddTicks(6033));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 15, 402, DateTimeKind.Utc).AddTicks(6035));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 15, 402, DateTimeKind.Utc).AddTicks(6137));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 16, 399, DateTimeKind.Utc).AddTicks(7309));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 15, 20, 45, 16, 400, DateTimeKind.Utc).AddTicks(1015));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$mBYICyN5WVz4c3BKIdZ/BOPdLFnNKtyhlDulDFlYKkcDFjtez5yCq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$mBYICyN5WVz4c3BKIdZ/BOPdLFnNKtyhlDulDFlYKkcDFjtez5yCq");
        }
    }
}
