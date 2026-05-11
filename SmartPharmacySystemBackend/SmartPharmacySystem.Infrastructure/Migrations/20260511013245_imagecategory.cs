using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class imagecategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 1, 32, 42, 141, DateTimeKind.Utc).AddTicks(8874));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 1, 32, 42, 142, DateTimeKind.Utc).AddTicks(1595));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 1, 32, 42, 142, DateTimeKind.Utc).AddTicks(1624));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 1, 32, 42, 142, DateTimeKind.Utc).AddTicks(1626));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 1, 32, 42, 142, DateTimeKind.Utc).AddTicks(1627));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 1, 32, 42, 142, DateTimeKind.Utc).AddTicks(1633));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 1, 32, 42, 142, DateTimeKind.Utc).AddTicks(1640));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 1, 32, 42, 142, DateTimeKind.Utc).AddTicks(1859));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 1, 32, 42, 142, DateTimeKind.Utc).AddTicks(1867));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$KFdn0eZZ0M3qM.woAwvEaeEqYThljAwmuXxfXuCvxNMLiM2dOULp6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$KFdn0eZZ0M3qM.woAwvEaeEqYThljAwmuXxfXuCvxNMLiM2dOULp6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Categories");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(2636));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4577));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4580));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4581));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4583));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4584));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4585));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4586));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4589));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ymQmErnZBKrcDjxAs7Ily.2dQdrNYiEOiFRip1HjpDr2EWkC5VOam");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$ymQmErnZBKrcDjxAs7Ily.2dQdrNYiEOiFRip1HjpDr2EWkC5VOam");
        }
    }
}
