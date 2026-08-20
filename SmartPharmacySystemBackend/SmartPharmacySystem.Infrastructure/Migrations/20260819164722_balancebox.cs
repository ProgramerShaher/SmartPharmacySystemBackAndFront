using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class balancebox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBalanceTransferredToSafe",
                table: "UserShifts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBalanceTransferredToSafe",
                table: "DailyClosings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 16, 605, DateTimeKind.Utc).AddTicks(9374));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 16, 606, DateTimeKind.Utc).AddTicks(598));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 16, 606, DateTimeKind.Utc).AddTicks(600));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 16, 606, DateTimeKind.Utc).AddTicks(601));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 16, 606, DateTimeKind.Utc).AddTicks(603));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 16, 606, DateTimeKind.Utc).AddTicks(605));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 16, 606, DateTimeKind.Utc).AddTicks(659));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 16, 606, DateTimeKind.Utc).AddTicks(661));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 16, 606, DateTimeKind.Utc).AddTicks(662));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 17, 305, DateTimeKind.Utc).AddTicks(6172));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 16, 47, 17, 305, DateTimeKind.Utc).AddTicks(9585));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$VG3.M0dpXCxqTduJaNuiYeckc/YqJcul2e1S5ZPRVgScLnqmrl7nm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$VG3.M0dpXCxqTduJaNuiYeckc/YqJcul2e1S5ZPRVgScLnqmrl7nm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBalanceTransferredToSafe",
                table: "UserShifts");

            migrationBuilder.DropColumn(
                name: "IsBalanceTransferredToSafe",
                table: "DailyClosings");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(4742));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5882));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5885));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5886));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5887));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5888));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5889));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5890));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5891));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 7, 685, DateTimeKind.Utc).AddTicks(2896));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 7, 685, DateTimeKind.Utc).AddTicks(5499));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Y2Lf1r88293U4l8s78/YGOTfWKTsBR/.SsMUU0J3rcF5dJUop/YxS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$Y2Lf1r88293U4l8s78/YGOTfWKTsBR/.SsMUU0J3rcF5dJUop/YxS");
        }
    }
}
