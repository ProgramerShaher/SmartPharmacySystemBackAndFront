using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class injouserandemployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Employees",
                type: "int",
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Users_UserId",
                table: "Employees",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Users_UserId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_UserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Employees");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 32, 858, DateTimeKind.Utc).AddTicks(2499));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 32, 858, DateTimeKind.Utc).AddTicks(4044));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 32, 858, DateTimeKind.Utc).AddTicks(4123));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 32, 858, DateTimeKind.Utc).AddTicks(4126));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 32, 858, DateTimeKind.Utc).AddTicks(4127));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 32, 858, DateTimeKind.Utc).AddTicks(4128));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 32, 858, DateTimeKind.Utc).AddTicks(4129));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 32, 858, DateTimeKind.Utc).AddTicks(4131));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 32, 858, DateTimeKind.Utc).AddTicks(4132));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 33, 813, DateTimeKind.Utc).AddTicks(2406));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 9, 0, 23, 33, 813, DateTimeKind.Utc).AddTicks(6499));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$3YqQkuISXO8M7IZjqgNxHu5mjF3ZoV2hUNBbsfxe07H3K0pb.Dclm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$3YqQkuISXO8M7IZjqgNxHu5mjF3ZoV2hUNBbsfxe07H3K0pb.Dclm");
        }
    }
}
