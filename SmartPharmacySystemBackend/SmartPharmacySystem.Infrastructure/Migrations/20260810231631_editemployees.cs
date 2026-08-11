using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editemployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Shift",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ShiftEndTime",
                table: "Employees",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ShiftStartTime",
                table: "Employees",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WorkingHours",
                table: "Employees",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 22, 371, DateTimeKind.Utc).AddTicks(2512));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 22, 371, DateTimeKind.Utc).AddTicks(4480));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 22, 371, DateTimeKind.Utc).AddTicks(4485));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 22, 371, DateTimeKind.Utc).AddTicks(4488));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 22, 371, DateTimeKind.Utc).AddTicks(4492));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 22, 371, DateTimeKind.Utc).AddTicks(4495));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 22, 371, DateTimeKind.Utc).AddTicks(4499));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 22, 371, DateTimeKind.Utc).AddTicks(4502));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 22, 371, DateTimeKind.Utc).AddTicks(4506));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 23, 275, DateTimeKind.Utc).AddTicks(965));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 23, 16, 23, 275, DateTimeKind.Utc).AddTicks(5775));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Ay7QunZsizqunn.ENjR5yemmVepSBO8E2p9zNIA93DyYTJMJyJQby");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$Ay7QunZsizqunn.ENjR5yemmVepSBO8E2p9zNIA93DyYTJMJyJQby");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Shift",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ShiftEndTime",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ShiftStartTime",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WorkingHours",
                table: "Employees");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 2, 652, DateTimeKind.Utc).AddTicks(8973));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 2, 653, DateTimeKind.Utc).AddTicks(298));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 2, 653, DateTimeKind.Utc).AddTicks(301));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 2, 653, DateTimeKind.Utc).AddTicks(302));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 2, 653, DateTimeKind.Utc).AddTicks(303));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 2, 653, DateTimeKind.Utc).AddTicks(304));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 2, 653, DateTimeKind.Utc).AddTicks(306));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 2, 653, DateTimeKind.Utc).AddTicks(307));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 2, 653, DateTimeKind.Utc).AddTicks(308));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 3, 565, DateTimeKind.Utc).AddTicks(5956));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 10, 14, 40, 3, 566, DateTimeKind.Utc).AddTicks(768));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ouvmzVLVwaIOdF3raxMiGuHWKSUm48ZyYlL/3AGMOoylCqV.w9xy2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$ouvmzVLVwaIOdF3raxMiGuHWKSUm48ZyYlL/3AGMOoylCqV.w9xy2");
        }
    }
}
