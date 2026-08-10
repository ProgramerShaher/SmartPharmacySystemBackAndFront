
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editusersandemployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_UserId",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "DefaultBranchId",
                table: "Users",
                type: "int",
                nullable: true);

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
                columns: new[] { "DefaultBranchId", "PasswordHash" },
                values: new object[] { null, "$2a$11$ouvmzVLVwaIOdF3raxMiGuHWKSUm48ZyYlL/3AGMOoylCqV.w9xy2" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DefaultBranchId", "PasswordHash" },
                values: new object[] { null, "$2a$11$ouvmzVLVwaIOdF3raxMiGuHWKSUm48ZyYlL/3AGMOoylCqV.w9xy2" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UserId",
                table: "Employees",
                column: "UserId",
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_UserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DefaultBranchId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 178, DateTimeKind.Utc).AddTicks(8337));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 178, DateTimeKind.Utc).AddTicks(9782));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 178, DateTimeKind.Utc).AddTicks(9841));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 178, DateTimeKind.Utc).AddTicks(9842));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 178, DateTimeKind.Utc).AddTicks(9844));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 178, DateTimeKind.Utc).AddTicks(9845));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 178, DateTimeKind.Utc).AddTicks(9846));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 178, DateTimeKind.Utc).AddTicks(9848));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 178, DateTimeKind.Utc).AddTicks(9849));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 903, DateTimeKind.Utc).AddTicks(7718));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 14, 25, 24, 904, DateTimeKind.Utc).AddTicks(798));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$zPjfNeUvm7X4Tv3tvqnI6euve1Mapwr/aOk3qELClQZvgR4F9EaZO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$zPjfNeUvm7X4Tv3tvqnI6euve1Mapwr/aOk3qELClQZvgR4F9EaZO");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UserId",
                table: "Employees",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }
    }
}
