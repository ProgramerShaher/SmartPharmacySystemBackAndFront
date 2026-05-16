using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Intialupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 172, DateTimeKind.Utc).AddTicks(4368));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 172, DateTimeKind.Utc).AddTicks(6903));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 172, DateTimeKind.Utc).AddTicks(6908));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 172, DateTimeKind.Utc).AddTicks(6911));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 172, DateTimeKind.Utc).AddTicks(6913));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 172, DateTimeKind.Utc).AddTicks(6915));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 172, DateTimeKind.Utc).AddTicks(6918));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 172, DateTimeKind.Utc).AddTicks(6920));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 172, DateTimeKind.Utc).AddTicks(6922));

            migrationBuilder.UpdateData(
                table: "JournalEntries",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsPosted",
                value: true);

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 940, DateTimeKind.Utc).AddTicks(6767));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 52, 4, 940, DateTimeKind.Utc).AddTicks(9999));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$NVYzryB8fGgw7herjOYl1uH6wpPWK/aLulVgm2T0iPmuf8uEmQ1Bq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$NVYzryB8fGgw7herjOYl1uH6wpPWK/aLulVgm2T0iPmuf8uEmQ1Bq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 28, 279, DateTimeKind.Utc).AddTicks(5705));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 28, 279, DateTimeKind.Utc).AddTicks(7249));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 28, 279, DateTimeKind.Utc).AddTicks(7252));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 28, 279, DateTimeKind.Utc).AddTicks(7254));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 28, 279, DateTimeKind.Utc).AddTicks(7256));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 28, 279, DateTimeKind.Utc).AddTicks(7257));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 28, 279, DateTimeKind.Utc).AddTicks(7259));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 28, 279, DateTimeKind.Utc).AddTicks(7261));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 28, 279, DateTimeKind.Utc).AddTicks(7262));

            migrationBuilder.UpdateData(
                table: "JournalEntries",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsPosted",
                value: false);

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 29, 1, DateTimeKind.Utc).AddTicks(5449));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 16, 0, 29, 1, DateTimeKind.Utc).AddTicks(8566));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$uvLLk6uV4bDc6L7Nx8ITBuM91LuLsaJQQPO8/T67avAqK8EMHUeNq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$uvLLk6uV4bDc6L7Nx8ITBuM91LuLsaJQQPO8/T67avAqK8EMHUeNq");
        }
    }
}
