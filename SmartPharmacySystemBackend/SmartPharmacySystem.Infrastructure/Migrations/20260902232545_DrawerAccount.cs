using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DrawerAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDrawerAccount",
                table: "PharmacyAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 37, 629, DateTimeKind.Utc).AddTicks(5796));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 37, 629, DateTimeKind.Utc).AddTicks(7202));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 37, 629, DateTimeKind.Utc).AddTicks(7206));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 37, 629, DateTimeKind.Utc).AddTicks(7210));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 37, 629, DateTimeKind.Utc).AddTicks(7213));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 37, 629, DateTimeKind.Utc).AddTicks(7216));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 37, 629, DateTimeKind.Utc).AddTicks(7219));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 37, 629, DateTimeKind.Utc).AddTicks(7222));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 37, 629, DateTimeKind.Utc).AddTicks(7225));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 38, 492, DateTimeKind.Utc).AddTicks(1527));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 23, 25, 38, 492, DateTimeKind.Utc).AddTicks(4025));

            migrationBuilder.UpdateData(
                table: "PharmacyAccounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsDrawerAccount",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$hb/YTtjbwMZ4w6Xl7X97ueoFs5i2bjWM17/vGKGnnSAcQz73BSzNq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$hb/YTtjbwMZ4w6Xl7X97ueoFs5i2bjWM17/vGKGnnSAcQz73BSzNq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDrawerAccount",
                table: "PharmacyAccounts");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 24, 797, DateTimeKind.Utc).AddTicks(9814));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 24, 798, DateTimeKind.Utc).AddTicks(500));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 24, 798, DateTimeKind.Utc).AddTicks(502));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 24, 798, DateTimeKind.Utc).AddTicks(503));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 24, 798, DateTimeKind.Utc).AddTicks(505));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 24, 798, DateTimeKind.Utc).AddTicks(562));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 24, 798, DateTimeKind.Utc).AddTicks(564));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 24, 798, DateTimeKind.Utc).AddTicks(565));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 24, 798, DateTimeKind.Utc).AddTicks(566));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 25, 487, DateTimeKind.Utc).AddTicks(1902));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 2, 19, 34, 25, 487, DateTimeKind.Utc).AddTicks(4462));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$q9Rh4GEALa0.ybepB5eLt.fiAh3FL2Ou/OApE1PTQMYa6qc9/Xzhi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$q9Rh4GEALa0.ybepB5eLt.fiAh3FL2Ou/OApE1PTQMYa6qc9/Xzhi");
        }
    }
}
